import type { Metadata } from "next";
import { GeistSans } from "geist/font/sans";
import Providers from "./providers";
import "./globals.css";

const geistSans = GeistSans;

export const metadata: Metadata = {
  title: "AulaIA — Asistente pedagógico MEP",
  description: "Planeamiento, asistencia y notas para docentes del MEP de Costa Rica.",
  icons: { icon: '/icon.png' },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es" className={`${geistSans.variable} h-full antialiased`}>
      <body className="min-h-full bg-gray-50 text-gray-900" suppressHydrationWarning>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
