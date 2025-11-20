"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import { EnhancedUserProfile, UpdateUserProfileRequest, SetOutOfOfficeRequest, PresenceStatus } from "@/types/userManagement";
import { getAccessToken, parseJwt } from "@/lib/session";
import SkillTagInput from "@/components/user-management/SkillTagInput";
import OutOfOfficeToggle from "@/components/user-management/OutOfOfficeToggle";
import UserAvatarWithPresence from "@/components/user-management/UserAvatarWithPresence";
import PresenceIndicator from "@/components/user-management/PresenceIndicator";

export default function ProfilePage() {
  const [userId, setUserId] = useState<string>("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [profile, setProfile] = useState<EnhancedUserProfile | null>(null);
  
  const [avatarUrl, setAvatarUrl] = useState("");
  const [bio, setBio] = useState("");
  const [linkedInUrl, setLinkedInUrl] = useState("");
  const [twitterUrl, setTwitterUrl] = useState("");
  const [skills, setSkills] = useState<string[]>([]);
  const [presenceStatus, setPresenceStatus] = useState<PresenceStatus>("Offline");

  useEffect(() => {
    const token = getAccessToken();
    const jwt = parseJwt(token);
    setUserId(jwt?.sub || jwt?.userId || "");
  }, []);

  useEffect(() => {
    if (userId) {
      loadProfile();
    }
  }, [userId]);

  const loadProfile = async () => {
    if (!getAccessToken() || !userId) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.profiles.getProfile(userId);
      const p = result.data;
      setProfile(p);
      setAvatarUrl(p.avatarUrl || "");
      setBio(p.bio || "");
      setLinkedInUrl(p.linkedInUrl || "");
      setTwitterUrl(p.twitterUrl || "");
      setSkills(p.skills || []);
      setPresenceStatus(p.presenceStatus as PresenceStatus);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        return;
      }
      setError(e.message || "Failed to load profile");
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    setSaving(true);
    try {
      const payload: UpdateUserProfileRequest = {
        avatarUrl: avatarUrl || undefined,
        bio: bio || undefined,
        linkedInUrl: linkedInUrl || undefined,
        twitterUrl: twitterUrl || undefined,
        skills: skills.length > 0 ? skills : undefined,
      };
      await UserManagementApi.profiles.updateProfile(userId, payload);
      setSuccess("Profile updated successfully");
      await loadProfile();
    } catch (e: any) {
      setError(e.message || "Failed to update profile");
    } finally {
      setSaving(false);
    }
  };

  const handleUpdatePresence = async (status: PresenceStatus) => {
    try {
      await UserManagementApi.profiles.updatePresence(userId, status);
      setPresenceStatus(status);
      await loadProfile();
    } catch (e: any) {
      alert(e.message || "Failed to update presence");
    }
  };

  const handleUpdateOOO = async (data: SetOutOfOfficeRequest) => {
    try {
      await UserManagementApi.profiles.setOutOfOffice(userId, data);
      await loadProfile();
    } catch (e: any) {
      throw new Error(e.message || "Failed to update out-of-office status");
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageName="My Profile" />
        <ComponentCard>
          <div className="text-center py-8">Loading profile...</div>
        </ComponentCard>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageName="My Profile" />
        <ComponentCard>
          <Alert color="danger">Profile not found</Alert>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageName="My Profile" />
      
      <ComponentCard className="mb-6">
        <div className="flex items-start gap-6 mb-6">
          <UserAvatarWithPresence
            name={`${profile.firstName} ${profile.lastName}`}
            avatarUrl={profile.avatarUrl}
            presenceStatus={presenceStatus}
            size="lg"
          />
          <div className="flex-1">
            <h1 className="text-2xl font-bold text-black dark:text-white mb-2">
              {profile.firstName} {profile.lastName}
            </h1>
            <p className="text-body-color dark:text-body-color-dark mb-3">{profile.email}</p>
            <div className="flex items-center gap-4">
              <PresenceIndicator status={presenceStatus} showLabel />
              <select
                value={presenceStatus}
                onChange={(e) => handleUpdatePresence(e.target.value as PresenceStatus)}
                className="px-3 py-1 border border-stroke rounded dark:bg-boxdark dark:border-strokedark"
              >
                <option value="Offline">Offline</option>
                <option value="Online">Online</option>
                <option value="Busy">Busy</option>
                <option value="Away">Away</option>
              </select>
            </div>
          </div>
        </div>
      </ComponentCard>

      <ComponentCard className="mb-6">
        <h2 className="text-xl font-semibold text-black dark:text-white mb-4">Profile Information</h2>
        {error && <Alert color="danger" className="mb-4">{error}</Alert>}
        {success && <Alert color="success" className="mb-4">{success}</Alert>}
        
        <form onSubmit={handleUpdateProfile} className="space-y-4">
          <div>
            <Label htmlFor="avatarUrl">Avatar URL</Label>
            <Input
              id="avatarUrl"
              type="url"
              value={avatarUrl}
              onChange={(e) => setAvatarUrl(e.target.value)}
              disabled={saving}
            />
          </div>

          <div>
            <Label htmlFor="bio">Bio</Label>
            <textarea
              id="bio"
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              rows={4}
              className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-strokedark dark:bg-boxdark dark:text-white dark:focus:border-primary"
              disabled={saving}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="linkedInUrl">LinkedIn URL</Label>
              <Input
                id="linkedInUrl"
                type="url"
                value={linkedInUrl}
                onChange={(e) => setLinkedInUrl(e.target.value)}
                disabled={saving}
              />
            </div>
            <div>
              <Label htmlFor="twitterUrl">Twitter URL</Label>
              <Input
                id="twitterUrl"
                type="url"
                value={twitterUrl}
                onChange={(e) => setTwitterUrl(e.target.value)}
                disabled={saving}
              />
            </div>
          </div>

          <div>
            <Label>Skills</Label>
            <SkillTagInput
              skills={skills}
              onChange={setSkills}
            />
          </div>

          <Button type="submit" color="primary" disabled={saving}>
            {saving ? "Saving..." : "Update Profile"}
          </Button>
        </form>
      </ComponentCard>

      <ComponentCard>
        <h2 className="text-xl font-semibold text-black dark:text-white mb-4">Out of Office</h2>
        <OutOfOfficeToggle
          isOutOfOffice={profile.outOfOfficeStatus}
          message={profile.outOfOfficeMessage}
          delegateUserId={profile.delegateUserId}
          onSubmit={handleUpdateOOO}
        />
      </ComponentCard>
    </div>
  );
}
