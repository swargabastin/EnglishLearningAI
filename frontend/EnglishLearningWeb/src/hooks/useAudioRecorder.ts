"use client";

import { useState, useRef, useCallback, useEffect } from "react";

export type RecorderState = "idle" | "requesting_permission" | "recording" | "processing" | "error";

export interface AudioRecorderHook {
  state: RecorderState;
  errorMessage: string | null;
  recordingDurationSec: number;
  liveTranscript: string;
  startRecording: () => Promise<void>;
  stopRecording: () => Promise<{ blob: Blob | null; transcript: string }>;
  cancelRecording: () => void;
  clearError: () => void;
  isSupported: boolean;
}

interface BrowserSpeechRecognition {
  continuous: boolean;
  interimResults: boolean;
  lang: string;
  onresult: ((event: Event) => void) | null;
  onerror: (() => void) | null;
  start: () => void;
  stop: () => void;
}

export function useAudioRecorder(): AudioRecorderHook {
  const [state, setState] = useState<RecorderState>("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [recordingDurationSec, setRecordingDurationSec] = useState<number>(0);
  const [isSupported, setIsSupported] = useState<boolean>(true);
  const [liveTranscript, setLiveTranscript] = useState<string>("");

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const streamRef = useRef<MediaStream | null>(null);
  const chunksRef = useRef<Blob[]>([]);
  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const recognitionRef = useRef<BrowserSpeechRecognition | null>(null);
  const transcriptAccumulatorRef = useRef<string>("");

  // Check browser MediaRecorder support
  useEffect(() => {
    if (typeof window !== "undefined") {
      const supported = !!(navigator.mediaDevices && window.MediaRecorder);
      setIsSupported(supported);
      if (!supported) {
        setErrorMessage("Microphone audio recording is not supported in this browser.");
      }
    }
  }, []);

  const cleanupStream = useCallback(() => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
    if (streamRef.current) {
      streamRef.current.getTracks().forEach((track) => track.stop());
      streamRef.current = null;
    }
    if (recognitionRef.current) {
      try {
        recognitionRef.current.stop();
      } catch {
        // Ignored
      }
      recognitionRef.current = null;
    }
    mediaRecorderRef.current = null;
    chunksRef.current = [];
    setRecordingDurationSec(0);
  }, []);

  useEffect(() => {
    return () => {
      cleanupStream();
    };
  }, [cleanupStream]);

  const getSupportedMimeType = (): string => {
    if (typeof MediaRecorder === "undefined") return "audio/webm";
    const candidates = [
      "audio/webm;codecs=opus",
      "audio/webm",
      "audio/mp4",
      "audio/ogg;codecs=opus",
      "audio/aac",
    ];
    for (const mime of candidates) {
      if (MediaRecorder.isTypeSupported(mime)) {
        return mime;
      }
    }
    return "";
  };

  const startRecording = useCallback(async () => {
    setErrorMessage(null);
    setLiveTranscript("");
    transcriptAccumulatorRef.current = "";
    setState("requesting_permission");

    if (!navigator.mediaDevices || !window.MediaRecorder) {
      setState("error");
      setErrorMessage("Audio recording is not supported on this browser.");
      return;
    }

    try {
      let stream: MediaStream;
      try {
        stream = await navigator.mediaDevices.getUserMedia({
          audio: {
            echoCancellation: true,
            noiseSuppression: true,
            autoGainControl: true,
          },
        });
      } catch {
        // Fallback: Some hardware or drivers fail when complex audio constraints are requested
        stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      }

      streamRef.current = stream;
      chunksRef.current = [];

      const mimeType = getSupportedMimeType();
      const options = mimeType ? { mimeType } : undefined;
      const mediaRecorder = new MediaRecorder(stream, options);

      mediaRecorder.ondataavailable = (event) => {
        if (event.data && event.data.size > 0) {
          chunksRef.current.push(event.data);
        }
      };

      mediaRecorderRef.current = mediaRecorder;
      mediaRecorder.start(200); // collect 200ms slices

      // Concurrently start Web Speech API recognition for instant browser transcription
      try {
        if (typeof window !== "undefined") {
          const win = window as unknown as Record<string, unknown>;
          const SpeechRecognitionClass = (win.SpeechRecognition || win.webkitSpeechRecognition) as
            | (new () => BrowserSpeechRecognition)
            | undefined;
          if (SpeechRecognitionClass) {
            const recognition = new SpeechRecognitionClass();
            recognition.continuous = true;
            recognition.interimResults = true;
            recognition.lang = "en-US";
            recognition.onresult = (event: Event) => {
              const recEvent = event as unknown as { results: { [key: number]: { [key: number]: { transcript: string } }; length: number } };
              let full = "";
              for (let i = 0; i < recEvent.results.length; i++) {
                full += recEvent.results[i][0].transcript + " ";
              }
              const trimmed = full.trim();
              if (trimmed) {
                transcriptAccumulatorRef.current = trimmed;
                setLiveTranscript(trimmed);
              }
            };
            recognition.onerror = () => {
              // Non-blocking fallback
            };
            recognition.start();
            recognitionRef.current = recognition;
          }
        }
      } catch {
        // Web Speech API is optional; MediaRecorder handles audio stream
      }

      setState("recording");
      setRecordingDurationSec(0);

      timerRef.current = setInterval(() => {
        setRecordingDurationSec((prev) => prev + 1);
      }, 1000);
    } catch (err: unknown) {
      cleanupStream();
      setState("error");
      const error = err as Error;
      if (error.name === "NotAllowedError" || error.name === "PermissionDeniedError") {
        setErrorMessage("Microphone access was denied. Please click the lock or camera icon in your browser address bar and allow microphone permissions.");
      } else if (error.name === "NotFoundError" || error.name === "DevicesNotFoundError") {
        setErrorMessage("No active microphone was detected by the browser. Check if your headset/mic is plugged in, selected in Windows Sound settings, or switch to Keyboard mode.");
      } else if (error.name === "NotReadableError" || error.name === "TrackStartError") {
        setErrorMessage("Your microphone is currently in use by another app (e.g. Teams, Zoom, or Discord) or blocked by exclusive mode.");
      } else {
        setErrorMessage("Unable to access microphone: " + (error.message || "Unknown error"));
      }
    }
  }, [cleanupStream]);

  const stopRecording = useCallback(async (): Promise<{ blob: Blob | null; transcript: string }> => {
    return new Promise((resolve) => {
      const recorder = mediaRecorderRef.current;
      const capturedTranscript = transcriptAccumulatorRef.current;

      if (!recorder || recorder.state === "inactive") {
        cleanupStream();
        setState("idle");
        resolve({ blob: null, transcript: capturedTranscript });
        return;
      }

      setState("processing");

      recorder.onstop = () => {
        const mimeType = recorder.mimeType || "audio/webm";
        const blob = new Blob(chunksRef.current, { type: mimeType });
        cleanupStream();
        setState("idle");
        resolve({
          blob: blob.size > 0 ? blob : null,
          transcript: capturedTranscript,
        });
      };

      try {
        recorder.stop();
      } catch {
        cleanupStream();
        setState("idle");
        resolve({ blob: null, transcript: capturedTranscript });
      }
    });
  }, [cleanupStream]);

  const cancelRecording = useCallback(() => {
    if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
      try {
        mediaRecorderRef.current.stop();
      } catch {
        // Ignored
      }
    }
    cleanupStream();
    setState("idle");
  }, [cleanupStream]);

  const clearError = useCallback(() => {
    setErrorMessage(null);
    setState("idle");
  }, []);

  return {
    state,
    errorMessage,
    recordingDurationSec,
    liveTranscript,
    startRecording,
    stopRecording,
    cancelRecording,
    clearError,
    isSupported,
  };
}
