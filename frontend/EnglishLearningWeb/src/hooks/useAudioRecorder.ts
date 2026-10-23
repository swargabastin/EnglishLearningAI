"use client";

import { useState, useRef, useCallback, useEffect } from "react";

export type RecorderState = "idle" | "requesting_permission" | "recording" | "processing" | "error";

export interface AudioRecorderOptions {
  onAutoSubmitTurn?: (blob: Blob, transcript: string) => Promise<void> | void;
  isAiSpeaking?: boolean;
  onBargeIn?: () => void;
  autoSilenceTimeoutMs?: number;
}

export interface AudioRecorderHook {
  state: RecorderState;
  errorMessage: string | null;
  recordingDurationSec: number;
  liveTranscript: string;
  isHandsFree: boolean;
  setIsHandsFree: (enabled: boolean) => void;
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
  onerror: ((event: Event) => void) | null;
  onend: (() => void) | null;
  start: () => void;
  stop: () => void;
  abort: () => void;
}

export function useAudioRecorder(options?: AudioRecorderOptions): AudioRecorderHook {
  const [state, setState] = useState<RecorderState>("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [recordingDurationSec, setRecordingDurationSec] = useState<number>(0);
  const [isSupported, setIsSupported] = useState<boolean>(true);
  const [liveTranscript, setLiveTranscript] = useState<string>("");
  const [isHandsFree, setIsHandsFree] = useState<boolean>(false);

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const streamRef = useRef<MediaStream | null>(null);
  const chunksRef = useRef<Blob[]>([]);
  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const recognitionRef = useRef<BrowserSpeechRecognition | null>(null);
  const transcriptAccumulatorRef = useRef<string>("");
  const isRecordingRef = useRef<boolean>(false);
  const isHandsFreeRef = useRef<boolean>(false);
  const silenceTimerRef = useRef<NodeJS.Timeout | null>(null);
  const optionsRef = useRef<AudioRecorderOptions | undefined>(options);
  const stopRecordingRef = useRef<() => Promise<{ blob: Blob | null; transcript: string }>>();

  useEffect(() => {
    optionsRef.current = options;
  }, [options]);

  useEffect(() => {
    isHandsFreeRef.current = isHandsFree;
  }, [isHandsFree]);

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
    isRecordingRef.current = false;
    if (silenceTimerRef.current) {
      clearTimeout(silenceTimerRef.current);
      silenceTimerRef.current = null;
    }
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
        recognitionRef.current.onend = null;
        recognitionRef.current.onerror = null;
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
      isRecordingRef.current = true;

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

      // Concurrently start Web Speech API recognition for high-accuracy local speech-to-text
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
              const recEvent = event as unknown as {
                results: {
                  [key: number]: {
                    isFinal?: boolean;
                    [key: number]: { transcript: string };
                  };
                  length: number;
                };
              };

              let finalPart = "";
              let interimPart = "";

              for (let i = 0; i < recEvent.results.length; i++) {
                const res = recEvent.results[i];
                if (res && res[0]) {
                  if (res.isFinal) {
                    finalPart += res[0].transcript + " ";
                  } else {
                    interimPart += res[0].transcript + " ";
                  }
                }
              }

              const full = (finalPart + interimPart).trim();
              if (full) {
                transcriptAccumulatorRef.current = full;
                setLiveTranscript(full);

                if (optionsRef.current?.isAiSpeaking) {
                  optionsRef.current.onBargeIn?.();
                }

                if (isHandsFreeRef.current) {
                  if (silenceTimerRef.current) {
                    clearTimeout(silenceTimerRef.current);
                  }
                  silenceTimerRef.current = setTimeout(async () => {
                    if (isRecordingRef.current && transcriptAccumulatorRef.current.trim().length > 0) {
                      const res = await stopRecordingRef.current?.();
                      if (res?.blob) {
                        await optionsRef.current?.onAutoSubmitTurn?.(res.blob, res.transcript);
                      }
                    }
                  }, optionsRef.current?.autoSilenceTimeoutMs || 1400);
                }
              }
            };

            recognition.onerror = (event: Event) => {
              const err = (event as unknown as { error?: string })?.error;
              if (err === "no-speech" || err === "aborted") {
                return;
              }
              console.warn("Speech recognition notice:", err);
            };

            recognition.onend = () => {
              // Automatically restart recognition if user is still actively recording
              if (isRecordingRef.current) {
                try {
                  recognition.start();
                } catch {
                  // Ignore if already starting
                }
              }
            };

            recognition.start();
            recognitionRef.current = recognition;
          }
        }
      } catch (e) {
        console.warn("Browser Web Speech API unavailable:", e);
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
        setErrorMessage("Microphone access was denied. Please click the lock icon in your browser address bar and allow microphone permissions.");
      } else if (error.name === "NotFoundError" || error.name === "DevicesNotFoundError") {
        setErrorMessage("No active microphone was detected by the browser. Check if your headset/mic is plugged in and selected in Windows Sound settings.");
      } else if (error.name === "NotReadableError" || error.name === "TrackStartError") {
        setErrorMessage("Your microphone is currently in use by another app or blocked by exclusive mode.");
      } else {
        setErrorMessage("Unable to access microphone: " + (error.message || "Unknown error"));
      }
    }
  }, [cleanupStream]);

  const stopRecording = useCallback(async (): Promise<{ blob: Blob | null; transcript: string }> => {
    return new Promise((resolve) => {
      isRecordingRef.current = false;
      const recorder = mediaRecorderRef.current;
      const capturedTranscript = transcriptAccumulatorRef.current;

      if (recognitionRef.current) {
        try {
          recognitionRef.current.onend = null;
          recognitionRef.current.stop();
        } catch {
          // Ignored
        }
      }

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
        const finalTranscript = transcriptAccumulatorRef.current || capturedTranscript;
        cleanupStream();
        setState("idle");
        resolve({
          blob: blob.size > 0 ? blob : null,
          transcript: finalTranscript,
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

  stopRecordingRef.current = stopRecording;

  const cancelRecording = useCallback(() => {
    isRecordingRef.current = false;
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
    isHandsFree,
    setIsHandsFree,
    startRecording,
    stopRecording,
    cancelRecording,
    clearError,
    isSupported,
  };
}
