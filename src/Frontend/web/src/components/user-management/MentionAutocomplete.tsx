"use client";
import React, { useState, useRef, useEffect } from "react";

interface User {
  userId: string;
  name: string;
  email: string;
}

interface MentionAutocompleteProps {
  value: string;
  onChange: (value: string) => void;
  onMention: (userId: string, userName: string) => void;
  users: User[];
  placeholder?: string;
}

export default function MentionAutocomplete({
  value,
  onChange,
  onMention,
  users,
  placeholder = "Type @ to mention someone...",
}: MentionAutocompleteProps) {
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [suggestions, setSuggestions] = useState<User[]>([]);
  const [mentionStart, setMentionStart] = useState<number | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = e.target.value;
    onChange(newValue);

    const cursorPos = e.target.selectionStart;
    const textBeforeCursor = newValue.substring(0, cursorPos);
    const mentionMatch = textBeforeCursor.match(/@(\w*)$/);

    if (mentionMatch) {
      const query = mentionMatch[1].toLowerCase();
      setMentionStart(cursorPos - mentionMatch[0].length);
      const filtered = users.filter(
        u => u.name.toLowerCase().includes(query) || u.email.toLowerCase().includes(query)
      );
      setSuggestions(filtered.slice(0, 5));
      setShowSuggestions(true);
    } else {
      setShowSuggestions(false);
      setSuggestions([]);
    }
  };

  const handleSelectUser = (user: User) => {
    if (mentionStart === null) return;
    
    const beforeMention = value.substring(0, mentionStart);
    const afterMention = value.substring(mentionStart).replace(/@\w*/, `@${user.name} `);
    const newValue = beforeMention + afterMention;
    
    onChange(newValue);
    onMention(user.userId, user.name);
    setShowSuggestions(false);
    setMentionStart(null);
    
    if (textareaRef.current) {
      textareaRef.current.focus();
      const newCursorPos = beforeMention.length + `@${user.name} `.length;
      textareaRef.current.setSelectionRange(newCursorPos, newCursorPos);
    }
  };

  return (
    <div className="relative">
      <textarea
        ref={textareaRef}
        value={value}
        onChange={handleChange}
        placeholder={placeholder}
        className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-strokedark dark:bg-boxdark dark:text-white dark:focus:border-primary"
        rows={4}
      />
      {showSuggestions && suggestions.length > 0 && (
        <div className="absolute z-10 mt-1 w-full rounded border border-stroke bg-white shadow-lg dark:border-strokedark dark:bg-boxdark">
          {suggestions.map((user) => (
            <button
              key={user.userId}
              type="button"
              onClick={() => handleSelectUser(user)}
              className="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-boxdark-2"
            >
              <div className="font-medium text-black dark:text-white">{user.name}</div>
              <div className="text-sm text-body-color dark:text-body-color-dark">{user.email}</div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

