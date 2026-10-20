"use client";

import React, { useState, useRef } from "react";
import { useRouter } from "next/navigation";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { adaptiveLearningService } from "@/services/adaptiveLearningService";
import { DocumentAnalysisResult } from "@/types";
import {
  FileText,
  Upload,
  Sparkles,
  BookOpen,
  MessageSquare,
  ArrowRight,
  Loader2,
  HelpCircle,
  FileCheck,
  Mic,
} from "lucide-react";

interface DocumentUploadModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function DocumentUploadModal({ isOpen, onClose }: DocumentUploadModalProps) {
  const router = useRouter();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [mode, setMode] = useState<"file" | "paste">("paste");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [docTitle, setDocTitle] = useState<string>("");
  const [docText, setDocText] = useState<string>("");
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [result, setResult] = useState<DocumentAnalysisResult | null>(null);

  const sampleTexts = [
    {
      title: "Remote Work Policy",
      text: "Our company embraces a flexible remote work model to enhance productivity and employee well-being. Team members are expected to attend weekly status meetings, communicate progress via chat channels, and uphold data security guidelines at all times. Collaboration across time zones requires proactive communication and clear objective setting.",
    },
    {
      title: "AI in Everyday Life",
      text: "Artificial intelligence has transformed modern society from automated recommendation engines to real-time speech translation. While machine learning algorithms improve efficiency in healthcare and education, ethical considerations regarding user privacy, transparency, and fairness remain critical topics of discussion.",
    },
  ];

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setSelectedFile(file);
      setDocTitle(file.name.replace(/\.[^/.]+$/, ""));
    }
  };

  const handleAnalyze = async () => {
    setIsLoading(true);
    try {
      let data: DocumentAnalysisResult;
      if (mode === "file" && selectedFile) {
        data = await adaptiveLearningService.uploadDocumentFile(selectedFile);
      } else {
        if (!docText.trim()) {
          alert("Please paste or type your document text.");
          setIsLoading(false);
          return;
        }
        data = await adaptiveLearningService.analyzeDocumentText(docTitle || "Uploaded English Document", docText);
      }
      setResult(data);
    } catch (err: unknown) {
      alert((err as Error).message || "Failed to analyze document.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleDiscussWithEmma = () => {
    if (!result) return;
    onClose();
    router.push(
      `/practice?topicId=22222222-2222-2222-2222-222222222222&objective=${encodeURIComponent(
        `Discussing "${result.title}": ${result.starterPrompt}`
      )}`
    );
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Document & Text Discussion Assistant">
      <div className="space-y-5 py-1 max-w-lg mx-auto">
        {/* Mode Selector */}
        <div className="flex items-center justify-center p-1 bg-slate-900 border border-slate-800 rounded-xl">
          <button
            type="button"
            onClick={() => setMode("paste")}
            className={`flex-1 py-1.5 text-xs font-bold rounded-lg transition-all ${
              mode === "paste"
                ? "bg-amber-600 text-white shadow-sm"
                : "text-slate-400 hover:text-slate-200"
            }`}
          >
            Paste Text / Articles
          </button>
          <button
            type="button"
            onClick={() => setMode("file")}
            className={`flex-1 py-1.5 text-xs font-bold rounded-lg transition-all ${
              mode === "file"
                ? "bg-amber-600 text-white shadow-sm"
                : "text-slate-400 hover:text-slate-200"
            }`}
          >
            Upload File (.txt, .pdf)
          </button>
        </div>

        {/* Mode: Paste Text */}
        {mode === "paste" ? (
          <div className="space-y-3">
            <input
              type="text"
              value={docTitle}
              onChange={(e) => setDocTitle(e.target.value)}
              placeholder="Document or Topic Title (optional)"
              className="w-full px-3.5 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:ring-1 focus:ring-amber-500"
            />
            <textarea
              value={docText}
              onChange={(e) => setDocText(e.target.value)}
              placeholder="Paste English article, email draft, or report text here..."
              rows={4}
              className="w-full px-4 py-3 bg-slate-900 border border-slate-700/80 rounded-2xl text-xs text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-amber-500 resize-none leading-relaxed"
            />

            {/* Quick Sample Articles */}
            <div className="flex flex-wrap items-center gap-1.5">
              <span className="text-[11px] text-slate-400 mr-1">Load sample:</span>
              {sampleTexts.map((s, idx) => (
                <button
                  key={idx}
                  type="button"
                  onClick={() => {
                    setDocTitle(s.title);
                    setDocText(s.text);
                  }}
                  className="px-2.5 py-1 rounded-xl bg-slate-800 hover:bg-slate-750 border border-slate-700 text-[11px] font-medium text-slate-300"
                >
                  {s.title}
                </button>
              ))}
            </div>
          </div>
        ) : (
          /* Mode: File Upload */
          <div className="space-y-3">
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleFileChange}
              accept=".txt,.pdf,.docx"
              className="hidden"
            />
            <div
              onClick={() => fileInputRef.current?.click()}
              className="p-6 border-2 border-dashed border-slate-700 hover:border-amber-500/60 bg-slate-900/60 hover:bg-slate-900 rounded-2xl text-center cursor-pointer transition-all space-y-2"
            >
              <div className="w-10 h-10 rounded-2xl bg-amber-950/50 border border-amber-800/40 text-amber-400 flex items-center justify-center mx-auto">
                <Upload className="w-5 h-5" />
              </div>
              <p className="text-xs font-bold text-white">
                {selectedFile ? selectedFile.name : "Click to select a document"}
              </p>
              <p className="text-[11px] text-slate-400">
                Supports meeting notes, articles, or transcripts (.txt, .pdf)
              </p>
            </div>
          </div>
        )}

        {/* Action Button */}
        <Button
          variant="primary"
          size="md"
          disabled={isLoading || (mode === "paste" && !docText.trim()) || (mode === "file" && !selectedFile)}
          onClick={handleAnalyze}
          className="w-full bg-amber-600 hover:bg-amber-500 text-white font-bold py-2.5 rounded-xl shadow-md"
        >
          {isLoading ? (
            <div className="flex items-center justify-center space-x-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              <span>Analyzing Document & Preparing Discussion...</span>
            </div>
          ) : (
            <div className="flex items-center justify-center space-x-2">
              <Sparkles className="w-4 h-4" />
              <span>Analyze & Extract Vocabulary</span>
            </div>
          )}
        </Button>

        {/* Analysis Result */}
        {result && (
          <div className="space-y-4 pt-2 border-t border-slate-800 animate-in fade-in duration-200">
            {/* Header with CEFR badge */}
            <div className="flex items-center justify-between p-3.5 bg-slate-850 rounded-2xl border border-slate-800">
              <div className="space-y-0.5">
                <span className="text-[10px] font-extrabold text-amber-400 uppercase tracking-wider">
                  Analyzed Topic
                </span>
                <p className="text-sm font-bold text-white">{result.title}</p>
              </div>
              <span className="px-2.5 py-1 rounded-xl bg-amber-950/60 border border-amber-800/50 text-xs font-bold text-amber-300">
                Level: {result.cefrDifficulty}
              </span>
            </div>

            {/* Document Summary */}
            <div className="p-3.5 bg-slate-850/80 rounded-2xl border border-slate-800 space-y-1">
              <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wide">
                Key Summary
              </span>
              <p className="text-xs text-slate-200 leading-relaxed">{result.summary}</p>
            </div>

            {/* Extracted Key Vocabulary */}
            {result.keyVocabulary && result.keyVocabulary.length > 0 && (
              <div className="space-y-1.5">
                <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wide">
                  Key Vocabulary in Context
                </span>
                <div className="space-y-1.5">
                  {result.keyVocabulary.map((v, idx) => (
                    <div
                      key={idx}
                      className="p-2.5 bg-slate-850 rounded-xl border border-slate-800 text-xs space-y-0.5"
                    >
                      <div className="flex items-center justify-between">
                        <span className="font-bold text-amber-300">{v.word}</span>
                        <span className="text-[11px] text-slate-400">{v.meaning}</span>
                      </div>
                      <p className="text-[11px] text-slate-300 italic">&quot;{v.exampleUsage}&quot;</p>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Discussion Questions */}
            {result.discussionQuestions && result.discussionQuestions.length > 0 && (
              <div className="p-3.5 bg-amber-950/30 border border-amber-800/40 rounded-2xl space-y-2">
                <span className="text-[11px] font-bold text-amber-300 uppercase tracking-wide flex items-center space-x-1.5">
                  <HelpCircle className="w-3.5 h-3.5" />
                  <span>Discussion Questions for Speaking Practice</span>
                </span>
                <ul className="space-y-1 text-xs text-slate-200 list-disc list-inside">
                  {result.discussionQuestions.map((q, idx) => (
                    <li key={idx}>{q}</li>
                  ))}
                </ul>
              </div>
            )}

            {/* Primary CTA: Discuss with Emma */}
            <div className="pt-2">
              <Button
                variant="primary"
                size="md"
                onClick={handleDiscussWithEmma}
                className="w-full bg-gradient-to-r from-amber-600 to-purple-600 hover:from-amber-500 hover:to-purple-500 text-white font-bold py-2.5 rounded-xl shadow-lg"
              >
                <div className="flex items-center justify-center space-x-2">
                  <Mic className="w-4 h-4" />
                  <span>Discuss this Document with Emma</span>
                  <ArrowRight className="w-4 h-4" />
                </div>
              </Button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
}
