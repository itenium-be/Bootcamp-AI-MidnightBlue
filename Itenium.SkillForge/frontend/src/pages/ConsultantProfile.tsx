import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { Badge, Button } from '@itenium-forge/ui';
import { ArrowLeft, AlertTriangle, Clock, Mail, Users } from 'lucide-react';
import { fetchConsultant } from '@/api/client';

interface ConsultantProfileProps {
  userId: string;
}

export function ConsultantProfile({ userId }: ConsultantProfileProps) {
  const { t } = useTranslation();

  const {
    data: consultant,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['consultant', userId],
    queryFn: () => fetchConsultant(userId),
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
    .map((n) => n[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();

  const joinedDate = new Date(consultant.createdAt).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

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
    </div>
  );
}
