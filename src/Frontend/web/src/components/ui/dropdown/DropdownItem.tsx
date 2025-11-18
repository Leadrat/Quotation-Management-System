"use client";
import Link from "next/link";
import React from "react";

type AnchorProps = React.ComponentProps<typeof Link> & { onItemClick?: () => void };
type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & { onItemClick?: () => void };

type DropdownItemProps =
  | ({ tag: "a" } & AnchorProps)
  | ({ tag?: "button" } & ButtonProps);

export const DropdownItem: React.FC<DropdownItemProps> = (props) => {
  const { onItemClick, ...rest } = props as any;

  if ((props as any).tag === "a") {
    const anchorProps = rest as AnchorProps;
    return (
      <Link {...anchorProps} onClick={(e) => { onItemClick?.(); (anchorProps as any).onClick?.(e); }}>
        {anchorProps.children}
      </Link>
    );
  }

  const buttonProps = rest as ButtonProps;
  return (
    <button {...buttonProps} onClick={(e) => { onItemClick?.(); buttonProps.onClick?.(e); }}>
      {buttonProps.children}
    </button>
  );
};
