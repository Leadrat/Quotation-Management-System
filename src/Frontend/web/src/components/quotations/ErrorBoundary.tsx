"use client";
import React, { Component, ErrorInfo, ReactNode } from "react";

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class QuotationErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("Quotation error boundary caught an error:", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        this.props.fallback || (
          <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-center">
            <h3 className="text-lg font-semibold text-red-800 mb-2">
              Something went wrong
            </h3>
            <p className="text-red-600 mb-4">
              {this.state.error?.message || "An unexpected error occurred while loading quotations."}
            </p>
            <button
              onClick={() => {
                this.setState({ hasError: false, error: undefined });
                window.location.reload();
              }}
              className="rounded bg-red-600 px-4 py-2 text-white hover:bg-red-700"
            >
              Reload Page
            </button>
          </div>
        )
      );
    }

    return this.props.children;
  }
}

