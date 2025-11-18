"use client";
import React, { useEffect, useRef } from "react";

type DropdownProps = {
  isOpen: boolean;
  onClose: () => void;
  className?: string;
  children: React.ReactNode;
};

export const Dropdown: React.FC<DropdownProps> = ({ isOpen, onClose, className, children }) => {
  const ref = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (!ref.current) return;
      if (!ref.current.contains(e.target as Node)) onClose();
    }
    function handleEsc(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleEsc);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleEsc);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div ref={ref} className={className}>{children}</div>
  );
};
