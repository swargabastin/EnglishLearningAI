import type { Metadata } from "next";
import { Plus_Jakarta_Sans } from "next/font/google";
import "./globals.css";
import { AuthProvider } from "@/context/AuthContext";
import { LanguageProvider } from "@/context/LanguageContext";
import { ServiceWorkerRegister } from "@/components/pwa/ServiceWorkerRegister";

const jakarta = Plus_Jakarta_Sans({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700", "800"],
  variable: "--font-jakarta",
  display: "swap",
});

export const metadata: Metadata = {
  title: "FluentAI — Spoken English Practice with AI Coach Emma",
  description: "Accelerate your English speaking fluency with personalized real-time AI conversation and assessment.",
  manifest: "/manifest.json",
  icons: {
    icon: "/icon-192.svg",
    apple: "/icon-192.svg",
  },
};

export const viewport = {
  themeColor: "#7c3aed",
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={jakarta.variable}>
      <body className="min-h-screen flex flex-col font-sans antialiased bg-slate-950 text-slate-100 selection:bg-purple-600 selection:text-white">
        <AuthProvider>
          <LanguageProvider>
            <ServiceWorkerRegister />
            {children}
          </LanguageProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
