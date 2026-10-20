"use client";

import React, { useState } from "react";
import { Volume2, Sliders, Check } from "lucide-react";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/Card";

export function AudioPreferencesCard() {
  const [voice, setVoice] = useState<"alloy" | "nova" | "echo">("nova");
  const [speed, setSpeed] = useState<number>(1.0);
  const [autoPlay, setAutoPlay] = useState<boolean>(true);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base font-bold text-white">Audio & Speech Preferences</CardTitle>
        <CardDescription>
          Customize AI coach voice synthesis, playback pacing, and accessibility controls.
        </CardDescription>
      </CardHeader>

      <CardContent className="space-y-5">
        {/* Tutor Voice */}
        <div className="space-y-2">
          <label className="block text-xs font-bold text-slate-300">
            Tutor Voice Persona
          </label>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            {[
              { id: "nova", name: "Emma (Nova)", desc: "Warm, natural & encouraging" },
              { id: "alloy", name: "Sarah (Alloy)", desc: "Neutral & precise" },
              { id: "echo", name: "David (Echo)", desc: "Deep & articulate" },
            ].map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setVoice(item.id as any)}
                className={`p-3.5 rounded-2xl border text-left transition-all ${
                  voice === item.id
                    ? "border-purple-500 bg-purple-950/40 ring-1 ring-purple-500 text-white font-bold"
                    : "border-slate-800 hover:border-slate-700 text-slate-300 bg-slate-850"
                }`}
              >
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold text-white">{item.name}</span>
                  {voice === item.id && <Check className="w-3.5 h-3.5 text-purple-400" />}
                </div>
                <p className="text-[11px] text-slate-400 font-normal mt-0.5">{item.desc}</p>
              </button>
            ))}
          </div>
        </div>

        {/* Playback Speed */}
        <div className="space-y-2">
          <label className="block text-xs font-bold text-slate-300">
            Default Audio Playback Speed
          </label>
          <div className="flex items-center space-x-2">
            {[0.8, 1.0, 1.2].map((s) => (
              <button
                key={s}
                type="button"
                onClick={() => setSpeed(s)}
                className={`px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                  speed === s
                    ? "bg-purple-600 text-white shadow-md shadow-purple-950/40"
                    : "bg-slate-800 hover:bg-slate-750 text-slate-300 border border-slate-700"
                }`}
              >
                {s}x {s === 1.0 ? "(Normal)" : s < 1.0 ? "(Slower)" : "(Faster)"}
              </button>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
