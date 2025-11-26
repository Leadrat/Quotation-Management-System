import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactCompiler: true,
  devIndicators: false,
  env: {
    NEXT_PUBLIC_API_BASE_URL: 'http://localhost:5001'
  },
  webpack: (config) => {
    // Exclude SVGs from the default next/image or file loader
    const fileLoaderRule = config.module.rules.find(
      // @ts-ignore
      (rule) => rule.test && rule.test.test && rule.test.test(".svg")
    );
    if (fileLoaderRule) {
      // @ts-ignore
      fileLoaderRule.exclude = /\.svg$/i;
    }

    // Use SVGR for SVG imports as React components
    config.module.rules.push({
      test: /\.svg$/i,
      use: [{ loader: "@svgr/webpack", options: { icon: true } }],
    });
    return config;
  },
};

export default nextConfig;
