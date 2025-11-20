"use client";
import React from "react";
import { TeamMember } from "@/types/userManagement";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface TeamMemberListProps {
  members: TeamMember[];
  onRemove?: (userId: string) => void;
  canRemove?: boolean;
}

export default function TeamMemberList({ members, onRemove, canRemove = false }: TeamMemberListProps) {
  if (members.length === 0) {
    return (
      <div className="text-center py-8 text-body-color dark:text-body-color-dark">
        No members in this team
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-stroke bg-white dark:border-strokedark dark:bg-boxdark">
      <Table>
        <TableHeader>
          <TableRow>
            <TableCell className="font-medium">Name</TableCell>
            <TableCell className="font-medium">Email</TableCell>
            <TableCell className="font-medium">Role</TableCell>
            <TableCell className="font-medium">Joined</TableCell>
            {canRemove && <TableCell className="font-medium">Actions</TableCell>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {members.map((member) => (
            <TableRow key={member.teamMemberId}>
              <TableCell className="text-black dark:text-white">{member.userName}</TableCell>
              <TableCell className="text-body-color dark:text-body-color-dark">{member.userEmail}</TableCell>
              <TableCell>
                <Badge color="primary" className="text-xs">{member.role}</Badge>
              </TableCell>
              <TableCell className="text-body-color dark:text-body-color-dark">
                {new Date(member.joinedAt).toLocaleDateString()}
              </TableCell>
              {canRemove && onRemove && (
                <TableCell>
                  <button
                    onClick={() => onRemove(member.userId)}
                    className="text-danger hover:text-danger-dark text-sm"
                    aria-label="Remove member"
                  >
                    Remove
                  </button>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

