"use client";

import React, { createContext, useContext, useState, useEffect } from "react";

export type Language = "en" | "ml";

export interface Translations {
  greeting: (name: string) => string;
  subheading: string;
  talkToEmmaCta: string;
  talkToEmmaSub: string;
  navHome: string;
  navPlan: string;
  navTopics: string;
  navVocabulary: string;
  navPractice: string;
  navHistory: string;
  navProfile: string;
  quickPracticeTitle: string;
  quickPracticeSub: string;
  quickTranslateTitle: string;
  quickTranslateSub: string;
  quickVocabTitle: string;
  quickVocabSub: string;
  quickEmailTitle: string;
  quickEmailSub: string;
  quickUploadTitle: string;
  quickUploadSub: string;
  comingSoon: string;
  todaysProgress: string;
  dailyGoal: string;
  streak: (days: number) => string;
  level: (lvl: string) => string;
  startPractice: string;
  recommendedForYou: string;
  learningHistory: string;
  noSessionsYet: string;
  startWithEmma: string;
  learnerProfile: string;
  signOut: string;
  practiceRoomTitle: string;
  tapToSpeak: string;
  stopSpeaking: string;
  useKeyboard: string;
  startSpeaking: string;
  tryAgain: string;
  sessionEnded: string;
  viewAssessment: string;
  backToDashboard: string;
}

const translations: Record<Language, Translations> = {
  en: {
    greeting: (name: string) => `Hello, ${name} 👋`,
    subheading: "How can I help you improve your English today?",
    talkToEmmaCta: "Talk to Emma",
    talkToEmmaSub: "Speak naturally. Get instant guidance.",
    navHome: "Home",
    navPlan: "Study Plan",
    navTopics: "Learn",
    navVocabulary: "Vocabulary",
    navPractice: "Practice",
    navHistory: "History",
    navProfile: "Profile",
    quickPracticeTitle: "Conversation Practice",
    quickPracticeSub: "Practice speaking real scenarios with Emma",
    quickTranslateTitle: "Translate to English",
    quickTranslateSub: "Translate Malayalam or other languages to fluent English",
    quickVocabTitle: "Learn a New Word",
    quickVocabSub: "Build your English vocabulary with active context",
    quickEmailTitle: "Email / Message",
    quickEmailSub: "Write better professional English messages",
    quickUploadTitle: "Photo / PDF",
    quickUploadSub: "Upload documents and ask Emma for explanations",
    comingSoon: "Coming Soon",
    todaysProgress: "Today's Progress",
    dailyGoal: "Daily Speaking Goal",
    streak: (days: number) => `${days} Day Streak`,
    level: (lvl: string) => `Level ${lvl}`,
    startPractice: "Start Practice",
    recommendedForYou: "Recommended for You",
    learningHistory: "Your Learning History",
    noSessionsYet: "No practice sessions yet.",
    startWithEmma: "Start practicing with Emma",
    learnerProfile: "Learner Profile & Settings",
    signOut: "Sign Out",
    practiceRoomTitle: "Speaking Practice Room",
    tapToSpeak: "Tap to Speak",
    stopSpeaking: "Tap to Finish",
    useKeyboard: "Use Keyboard",
    startSpeaking: "Start Speaking",
    tryAgain: "Try Again",
    sessionEnded: "Speaking Practice Completed",
    viewAssessment: "View Assessment & Feedback",
    backToDashboard: "Dashboard",
  },
  ml: {
    greeting: (name: string) => `നമസ്കാരം, ${name} 👋`,
    subheading: "ഇന്ന് നിങ്ങളുടെ ഇംഗ്ലീഷ് മെച്ചപ്പെടുത്താൻ ഞാൻ എങ്ങനെ സഹായിക്കണം?",
    talkToEmmaCta: "എമ്മയോട് സംസാരിക്കുക",
    talkToEmmaSub: "സ്വാഭാവികമായി സംസാരിക്കൂ. തത്സമയം സഹായം നേടൂ.",
    navHome: "ഡാഷ്‌ബോർഡ്",
    navPlan: "പഠന പദ്ധതി",
    navTopics: "പഠിക്കുക",
    navVocabulary: "പദാവലി",
    navPractice: "പരിശീലനം",
    navHistory: "ചരിത്രം",
    navProfile: "പ്രൊഫൈൽ",
    quickPracticeTitle: "സംഭാഷണ പരിശീലനം",
    quickPracticeSub: "എമ്മയോടൊപ്പം യഥാർത്ഥ സംഭാഷണങ്ങൾ പരിശീലിക്കുക",
    quickTranslateTitle: "ഇംഗ്ലീഷിലേക്ക് വിവർത്തനം ചെയ്യുക",
    quickTranslateSub: "മലയാളം വാചകങ്ങൾ ശരിയായ ഇംഗ്ലീഷിലേക്ക് മാറ്റുക",
    quickVocabTitle: "പുതിയ വാക്ക് പഠിക്കുക",
    quickVocabSub: "ഉദാഹരണങ്ങളോടെ പുതിയ വാക്കുകൾ പഠിക്കുക",
    quickEmailTitle: "ഇമെയിൽ / മെസ്സേജ്",
    quickEmailSub: "പ്രൊഫഷണൽ ഇംഗ്ലീഷ് സന്ദേശങ്ങൾ എഴുതുക",
    quickUploadTitle: "ഫോട്ടോ / PDF",
    quickUploadSub: "ഡോക്യുമെന്റുകൾ അപ്‌ലോഡ് ചെയ്ത് സംശയങ്ങൾ ചോദിക്കുക",
    comingSoon: "ഉടൻ ലഭ്യമാകും",
    todaysProgress: "ഇന്നത്തെ പുരോഗതി",
    dailyGoal: "പ്രതിദിന സംസാരിക്കൽ ലക്ഷ്യം",
    streak: (days: number) => `${days} ദിവസത്തെ സ്ട്രീക്ക്`,
    level: (lvl: string) => `ലെവൽ ${lvl}`,
    startPractice: "പരിശീലനം ആരംഭിക്കുക",
    recommendedForYou: "നിങ്ങൾക്കായി ശുപാർശ ചെയ്യുന്നത്",
    learningHistory: "നിങ്ങളുടെ പഠന ചരിത്രം",
    noSessionsYet: "ഇതുവരെ പരിശീലന സെഷനുകളൊന്നുമില്ല.",
    startWithEmma: "എമ്മയോടൊപ്പം പരിശീലനം ആരംഭിക്കൂ",
    learnerProfile: "പഠിതാവിന്റെ പ്രൊഫൈൽ",
    signOut: "പുറത്തുകടക്കുക",
    practiceRoomTitle: "സംഭാഷണ പരിശീലന മുറി",
    tapToSpeak: "സംസാരിക്കാൻ അമർത്തുക",
    stopSpeaking: "പൂർത്തിയാക്കാൻ അമർത്തുക",
    useKeyboard: "കീബോർഡ് ഉപയോഗിക്കുക",
    startSpeaking: "സംസാരിക്കാൻ തുടങ്ങുക",
    tryAgain: "വീണ്ടും ശ്രമിക്കുക",
    sessionEnded: "പരിശീലനം പൂർത്തിയായി",
    viewAssessment: "ഫീഡ്‌ബാക്കും മാർക്കും കാണുക",
    backToDashboard: "ഡാഷ്‌ബോർഡ്",
  },
};

interface LanguageContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  t: Translations;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export function LanguageProvider({ children }: { children: React.ReactNode }) {
  const [language, setLanguageState] = useState<Language>("en");

  useEffect(() => {
    const saved = localStorage.getItem("fluentai_language") as Language | null;
    if (saved === "en" || saved === "ml") {
      setLanguageState(saved);
    }
  }, []);

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    localStorage.setItem("fluentai_language", lang);
  };

  return (
    <LanguageContext.Provider value={{ language, setLanguage, t: translations[language] }}>
      {children}
    </LanguageContext.Provider>
  );
}

export function useLanguage() {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error("useLanguage must be used within a LanguageProvider");
  }
  return context;
}
