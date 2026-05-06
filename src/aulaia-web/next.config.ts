import type { NextConfig } from "next";
import { PHASE_DEVELOPMENT_SERVER } from "next/constants";

export default function config(phase: string): NextConfig {
  const base: NextConfig = {
    trailingSlash: true,
    images: { unoptimized: true },
  };

  if (phase === PHASE_DEVELOPMENT_SERVER) {
    // En dev: proxy /api/* → .NET en 5070 para que el browser vea todo en :3000.
    // No se usa output:export en dev (incompatible con rewrites).
    return {
      ...base,
      async rewrites() {
        return {
          beforeFiles: [
            {
              source: "/api/:path*",
              destination: "http://localhost:8000/api/:path*",
            },
          ],
        };
      },
    };
  }

  // En build: SPA estático para wwwroot/ (ADR-007)
  return { ...base, output: "export" };
}
