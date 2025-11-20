"use client";
import React, { Component, ErrorInfo, ReactNode } from "react";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Button from "@/components/tailadmin/ui/button/Button";

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ProductErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("Product management error:", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <ComponentCard title="Error">
          <div className="rounded border border-red-500 bg-red-50 p-4 text-red-700 dark:bg-red-900/20 dark:text-red-400">
            <h3 className="mb-2 text-lg font-semibold">Something went wrong</h3>
            <p className="mb-4">
              {this.state.error?.message || "An unexpected error occurred while loading product data."}
            </p>
            <Button
              onClick={() => {
                this.setState({ hasError: false, error: null });
                window.location.reload();
              }}
            >
              Reload Page
            </Button>
          </div>
        </ComponentCard>
      );
    }

    return this.props.children;
  }
}

