import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { Badge, Button } from '@itenium-forge/ui';
import { AlertTriangle, Clock, Users } from 'lucide-react';
import { fetchConsultants, type ConsultantSummary } from '@/api/client';

function ActivityBadge({ consultant }: { consultant: ConsultantSummary }) {
  const { t } = useTranslation();

  if (consultant.isInactive) {
    const label =
      consultant.daysSinceActivity != null
        ? t('team.inactiveDays', { count: consultant.daysSinceActivity })
        : t('team.neverActive');
    return (
      <Badge variant="destructive" className="gap-1">
        <AlertTriangle className="size-3" />
        {label}
      </Badge>
    );
  }

  if (consultant.daysSinceActivity != null) {
    return (
      <Badge variant="secondary" className="gap-1">
        <Clock className="size-3" />
        {t('team.activeDaysAgo', { count: consultant.daysSinceActivity })}
      </Badge>
    );
  }

  return null;
}

function ConsultantCard({ consultant }: { consultant: ConsultantSummary }) {
  const { t } = useTranslation();

  return (
    <div
      className={`rounded-lg border p-4 flex items-center justify-between gap-4 ${
        consultant.isInactive ? 'border-destructive/40 bg-destructive/5' : ''
      }`}
    >
      <div className="flex items-center gap-3 min-w-0">
        <div className="size-10 rounded-full bg-muted flex items-center justify-center shrink-0 font-semibold text-sm">
          {consultant.displayName
            .split(' ')
            .map((n) => n[0])
            .slice(0, 2)
            .join('')
            .toUpperCase()}
        </div>
        <div className="min-w-0">
          <p className="font-medium truncate">{consultant.displayName}</p>
          <p className="text-sm text-muted-foreground truncate">{consultant.email}</p>
        </div>
      </div>

      <div className="flex items-center gap-3 shrink-0">
        <Badge variant="outline">{consultant.teamName}</Badge>
        <ActivityBadge consultant={consultant} />
        <Button size="sm" variant="ghost" asChild>
          <Link to="/consultant/$userId" params={{ userId: consultant.userId }}>
            {t('team.viewProfile')}
          </Link>
        </Button>
      </div>
    </div>
  );
}

export function TeamMembers() {
  const { t } = useTranslation();

  const { data: consultants, isLoading } = useQuery({
    queryKey: ['consultants'],
    queryFn: fetchConsultants,
  });

  const inactive = consultants?.filter((c) => c.isInactive) ?? [];
  const active = consultants?.filter((c) => !c.isInactive) ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('team.title')}</h1>
          <p className="text-muted-foreground mt-1">{t('team.subtitle')}</p>
        </div>
        {consultants && (
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Users className="size-4" />
            <span>{t('team.totalConsultants', { count: consultants.length })}</span>
          </div>
        )}
      </div>

      {isLoading && <p className="text-muted-foreground">{t('common.loading')}</p>}

      {!isLoading && consultants?.length === 0 && <p className="text-muted-foreground">{t('team.noConsultants')}</p>}

      {/* Inactive section */}
      {inactive.length > 0 && (
        <div className="space-y-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-destructive flex items-center gap-2">
            <AlertTriangle className="size-4" />
            {t('team.inactiveSection', { count: inactive.length })}
          </h2>
          <div className="space-y-2">
            {inactive.map((c) => (
              <ConsultantCard key={c.userId} consultant={c} />
            ))}
          </div>
        </div>
      )}

      {/* Active section */}
      {active.length > 0 && (
        <div className="space-y-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground flex items-center gap-2">
            <Clock className="size-4" />
            {t('team.activeSection', { count: active.length })}
          </h2>
          <div className="space-y-2">
            {active.map((c) => (
              <ConsultantCard key={c.userId} consultant={c} />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
