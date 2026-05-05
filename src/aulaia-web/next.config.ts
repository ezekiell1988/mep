import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // ADR-007: SPA estático servido por .NET 10 desde wwwroot/
  output: "export",
  // Rutas relativas para que funcione desde cualquier subpath cuando .NET sirve los assets
  trailingSlash: true,
  // Sin image optimization (no hay servidor Next.js en prod)
  images: {
    unoptimized: true,
  },
};

export default nextConfig;
