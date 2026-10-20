import { ConversationMessage } from "@/types";

export const mockInitialMessages: ConversationMessage[] = [
  {
    id: "msg-1",
    role: "assistant",
    content: "Hello Alex! Welcome to today's Job Interview practice session. To start off, could you tell me a little bit about yourself and your professional background?",
    timestamp: "10:00 AM",
    audioDurationSec: 6
  },
  {
    id: "msg-2",
    role: "user",
    content: "Hi! I am working as a software developer since 2022. I build web applications and manage small project teams.",
    timestamp: "10:01 AM",
    audioDurationSec: 5
  },
  {
    id: "msg-3",
    role: "assistant",
    content: "That sounds like valuable experience! Can you describe a challenging project you worked on and how you handled unexpected obstacles?",
    timestamp: "10:01 AM",
    audioDurationSec: 7
  },
  {
    id: "msg-4",
    role: "user",
    content: "Last year, we had a very tight deadline for a client release. Our main database crashed two days before launch, so I coordinated with the team to migrate the database and we delivered on time.",
    timestamp: "10:03 AM",
    audioDurationSec: 10
  },
  {
    id: "msg-5",
    role: "assistant",
    content: "Impressive problem-solving! How do you usually communicate with stakeholders when technical issues cause potential delays?",
    timestamp: "10:04 AM",
    audioDurationSec: 6
  }
];

export const mockNextAiResponses = [
  "Thank you for sharing that. Clear stakeholder communication is crucial in leadership roles. What do you consider your greatest strength as a team collaborator?",
  "That makes total sense. Could you share an example of a time you received constructive feedback from a manager and how you applied it?",
  "Wonderful answer! Finally, what are your long-term career goals for the next two to three years?"
];
