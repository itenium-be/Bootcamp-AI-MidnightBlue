import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { Badge, Button } from '@itenium-forge/ui';
import { ArrowLeft, AlertTriangle, Clock, Mail, Users, ChevronRight } from 'lucide-react';
import {
  fetchConsultant,
  fetchProfiles,
  assignConsultantProfile,
  fetchConsultantSkills,
} from '@/api/client';
import { useAuthStore } from '@/stores';

interface ConsultantProfileProps {
  userId: string;
}

export function ConsultantProfile({ userId }: ConsultantProfileProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const canAssign = user?.isManager === true || user?.isBackOffice === true;

  // 'unset' means not yet changed by user — falls back to consultant.profileId
  const [draftProfileId, setDraftProfileId] = useState<number | null | 'unset'>('unset');

  const {
    data: consultant,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['consultant', userId],
    queryFn: () => fetchConsultant(userId),
  });

  const { data: profiles } = useQuery({
    queryKey: ['profiles'],
    queryFn: fetchProfiles,
    enabled: canAssign,
  });

  const { data: skillCategories } = useQuery({
    queryKey: ['consultant', userId, 'skills'],
    queryFn: () => fetchConsultantSkills(userId),
    enabled: !!consultant,
  });

  const assignMutation = useMutation({
    mutationFn: (profileId: number | null) => assignConsultantProfile(userId, profileId),
    onSuccess: () => {
      setDraftProfileId('unset');
      void queryClient.invalidateQueries({ queryKey: ['consultant', userId] });
      void queryClient.invalidateQueries({ queryKey: ['consultant', userId, 'skills'] });
    },
  });

  if (isLoading) {
    return <p className="text-muted-foreground">{t('common.loading')}</p>;
  }

  if (isError || !consultant) {
    return (
      <div className="space-y-4">
        <p className="text-destructive">{t('common.error')}</p>
        <Button variant="ghost" size="sm" asChild>
          <Link to="/team/members">
            <ArrowLeft className="size-4 mr-2" />
            {t('consultant.backToTeam')}
          </Link>
        </Button>
      </div>
    );
  }

  const initials = consultant.displayName
    .split(' ')
    .filter((n) => n.length > 0)
    .map((n) => n[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();

  const joinedDate = new Date(consultant.createdAt).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  const currentProfileId = draftProfileId === 'unset' ? consultant.profileId : draftProfileId;
  const isDirty = draftProfileId !== 'unset' && draftProfileId !== consultant.profileId;

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/team/members">
            <ArrowLeft className="size-4 mr-2" />
            {t('consultant.backToTeam')}
          </Link>
        </Button>
      </div>

      {/* Header card */}
      <div className="rounded-lg border p-6 space-y-4">
        <div className="flex items-start gap-4">
          <div className="size-16 rounded-full bg-muted flex items-center justify-center text-xl font-bold shrink-0">
            {initials}
          </div>
          <div className="space-y-1">
            <h1 className="text-2xl font-bold">{consultant.displayName}</h1>
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Mail className="size-4" />
              <span>{consultant.email}</span>
            </div>
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Users className="size-4" />
              <span>{consultant.teamName}</span>
            </div>
          </div>
        </div>

        <div className="flex flex-wrap gap-2 pt-2 border-t">
          <Badge variant="outline">{consultant.teamName}</Badge>
          {consultant.isInactive ? (
            <Badge variant="destructive" className="gap-1">
              <AlertTriangle className="size-3" />
              {consultant.daysSinceActivity != null
                ? t('team.inactiveDays', { count: consultant.daysSinceActivity })
                : t('team.neverActive')}
            </Badge>
          ) : (
            consultant.daysSinceActivity != null && (
              <Badge variant="secondary" className="gap-1">
                <Clock className="size-3" />
                {t('team.activeDaysAgo', { count: consultant.daysSinceActivity })}
              </Badge>
            )
          )}
        </div>
      </div>

      {/* Activity info */}
      <div className="rounded-lg border p-6 space-y-3">
        <h2 className="font-semibold">{t('consultant.activity')}</h2>
        <dl className="space-y-2 text-sm">
          <div className="flex justify-between">
            <dt className="text-muted-foreground">{t('consultant.joinedOn')}</dt>
            <dd>{joinedDate}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-muted-foreground">{t('consultant.lastActive')}</dt>
            <dd>
              {consultant.lastActivityAt
                ? new Date(consultant.lastActivityAt).toLocaleDateString(undefined, {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })
                : t('consultant.neverActive')}
            </dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-muted-foreground">{t('consultant.status')}</dt>
            <dd>
              {consultant.isInactive ? (
                <span className="text-destructive">{t('consultant.inactive')}</span>
              ) : (
                <span className="text-green-600 dark:text-green-400">{t('consultant.active')}</span>
              )}
            </dd>
          </div>
        </dl>
      </div>

      {/* Competence Centre Profile assignment */}
      <div className="rounded-lg border p-6 space-y-3">
        <h2 className="font-semibold">{t('consultant.profile')}</h2>
        {canAssign && profiles ? (
          <div className="flex items-center gap-3">
            <select
              className="flex-1 rounded-md border bg-background px-3 py-2 text-sm"
              value={currentProfileId ?? ''}
              onChange={(e) =>
                setDraftProfileId(e.target.value === '' ? null : Number(e.target.value))
              }
            >
              <option value="">{t('consultant.noProfile')}</option>
              {profiles.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                </option>
              ))}
            </select>
            <Button
              size="sm"
              disabled={!isDirty || assignMutation.isPending}
              onClick={() => assignMutation.mutate(draftProfileId === 'unset' ? null : (draftProfileId ?? null))}
            >
              {t('consultant.saveProfile')}
            </Button>
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">
            {consultant.profileName ?? t('consultant.noProfile')}
          </p>
        )}
        {assignMutation.isSuccess && draftProfileId === 'unset' && (
          <p className="text-sm text-green-600 dark:text-green-400">{t('consultant.profileSaved')}</p>
        )}
      </div>

      {/* Skill Roadmap */}
      <div className="rounded-lg border p-6 space-y-3">
        <h2 className="font-semibold">{t('consultant.skillRoadmap')}</h2>
        {!consultant.profileId ? (
          <p className="text-sm text-muted-foreground">{t('consultant.noRoadmap')}</p>
        ) : !skillCategories || skillCategories.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t('common.loading')}</p>
        ) : (
          <div className="space-y-4">
            {skillCategories.map((cat) => (
              <div key={cat.category}>
                <h3 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground mb-2">
                  {cat.category}
                </h3>
                <ul className="space-y-1">
                  {cat.skills.map((skill) => (
                    <li
                      key={skill.id}
                      className="flex items-center justify-between rounded-md px-3 py-2 text-sm hover:bg-muted/40"
                    >
                      <span className="flex items-center gap-2">
                        <ChevronRight className="size-3 text-muted-foreground shrink-0" />
                        {skill.name}
                      </span>
                      <Badge variant="outline" className="text-xs">
                        {skill.levelCount === 1 ? t('skills.checkbox') : t('skills.levels', { count: skill.levelCount })}
                      </Badge>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
