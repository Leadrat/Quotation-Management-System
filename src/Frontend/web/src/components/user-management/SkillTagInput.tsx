"use client";
import React, { useState } from "react";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface SkillTagInputProps {
  skills: string[];
  onChange: (skills: string[]) => void;
  placeholder?: string;
}

export default function SkillTagInput({ skills, onChange, placeholder = "Add a skill and press Enter" }: SkillTagInputProps) {
  const [inputValue, setInputValue] = useState("");

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && inputValue.trim()) {
      e.preventDefault();
      if (!skills.includes(inputValue.trim())) {
        onChange([...skills, inputValue.trim()]);
      }
      setInputValue("");
    } else if (e.key === "Backspace" && !inputValue && skills.length > 0) {
      onChange(skills.slice(0, -1));
    }
  };

  const removeSkill = (skillToRemove: string) => {
    onChange(skills.filter(s => s !== skillToRemove));
  };

  return (
    <div className="rounded border border-stroke bg-transparent p-3 dark:border-strokedark dark:bg-boxdark">
      <div className="flex flex-wrap gap-2 mb-2">
        {skills.map((skill) => (
          <Badge key={skill} color="primary" className="text-xs flex items-center gap-1">
            {skill}
            <button
              type="button"
              onClick={() => removeSkill(skill)}
              className="ml-1 hover:text-danger"
              aria-label={`Remove ${skill}`}
            >
              ×
            </button>
          </Badge>
        ))}
      </div>
      <input
        type="text"
        value={inputValue}
        onChange={(e) => setInputValue(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        className="w-full bg-transparent text-black outline-none dark:text-white"
      />
    </div>
  );
}

