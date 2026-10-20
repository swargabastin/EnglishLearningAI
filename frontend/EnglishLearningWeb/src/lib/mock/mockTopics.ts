import { Topic } from "@/types";

export const mockTopics: Topic[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    title: "Daily Conversation",
    description: "Casual everyday discussions about your routines, hobbies, weather, and weekend plans.",
    difficulty: "A1",
    category: "General",
    iconKey: "MessageCircle",
    estimatedMinutes: 10,
    popular: true,
    promptGuidance: "Focus on present tense, simple past, and natural friendly greetings."
  },
  {
    id: "22222222-2222-2222-2222-222222222222",
    title: "Job Interview",
    description: "Practice answering behavioral interview questions, describing your experience and career strengths.",
    difficulty: "B2",
    category: "Professional",
    iconKey: "Briefcase",
    estimatedMinutes: 15,
    popular: true,
    promptGuidance: "Practice professional vocabulary, STAR method explanations, and polite formal phrasing."
  },
  {
    id: "33333333-3333-3333-3333-333333333333",
    title: "Travel",
    description: "Simulate airport check-ins, asking for directions, hotel bookings, and navigating foreign cities.",
    difficulty: "B1",
    category: "Situational",
    iconKey: "Compass",
    estimatedMinutes: 12,
    popular: true,
    promptGuidance: "Useful phrases for inquiries, transportation, and polite requests."
  },
  {
    id: "44444444-4444-4444-4444-444444444444",
    title: "Shopping",
    description: "Inquiring about sizes, prices, returns, bargaining politely, and making purchases in retail stores.",
    difficulty: "A2",
    category: "Situational",
    iconKey: "ShoppingBag",
    estimatedMinutes: 10,
    promptGuidance: "Practice numbers, comparisons, asking for alternatives, and payment phrases."
  },
  {
    id: "55555555-5555-5555-5555-555555555555",
    title: "Restaurant",
    description: "Ordering meals, asking about ingredients, dietary requirements, requesting the bill, and complimenting the chef.",
    difficulty: "A2",
    category: "Situational",
    iconKey: "UtensilsCrossed",
    estimatedMinutes: 10,
    promptGuidance: "Polite modal verbs ('Could I have...', 'Would you recommend...')."
  },
  {
    id: "66666666-6666-6666-6666-666666666666",
    title: "Workplace",
    description: "Participating in team meetings, giving status updates, negotiating deadlines, and emailing colleagues.",
    difficulty: "B1",
    category: "Professional",
    iconKey: "Building2",
    estimatedMinutes: 15,
    promptGuidance: "Workplace idioms, project progress vocabulary, and diplomatic suggestions."
  },
  {
    id: "77777777-7777-7777-7777-777777777777",
    title: "Introducing Yourself",
    description: "Build confidence introducing your background, interests, profession, and family in personal or networking setups.",
    difficulty: "A1",
    category: "General",
    iconKey: "UserCheck",
    estimatedMinutes: 8,
    popular: true,
    promptGuidance: "Clear self-introduction structures and icebreakers."
  }
];
