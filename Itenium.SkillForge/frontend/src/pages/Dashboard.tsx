import { useTranslation } from 'react-i18next';
import { BookOpen, Users, Award, Flag } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardHeader, CardTitle, CardContent } from '@itenium-forge/ui';
import { useTeamStore } from '@/stores';
import { fetchTeamFlags, type TeamFlag } from '@/api/client';

function daysAgo(dateStr: string): number {
  return Math.floor((Date.now() - new Date(dateStr).getTime()) / 86_400_000);
}

export function Dashboard() {
  const { t } = useTranslation();
  const { mode, selectedTeam } = useTeamStore();
  const isManager = mode === 'manager';

  const { data: teamFlags = [] } = useQuery<TeamFlag[]>({
    queryKey: ['team-flags'],
    queryFn: fetchTeamFlags,
    enabled: isManager,
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('dashboard.title')}</h1>
        <p className="text-muted-foreground">
          {t('dashboard.welcome')}
          {isManager && selectedTeam && ` - ${selectedTeam.name}`}
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">{t('dashboard.totalCourses')}</CardTitle>
            <BookOpen className="size-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">24</div>
            <p className="text-xs text-muted-foreground">+3 from last month</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">{t('dashboard.activeLearners')}</CardTitle>
            <Users className="size-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">156</div>
            <p className="text-xs text-muted-foreground">Active this month</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">{t('dashboard.completedCourses')}</CardTitle>
            <Award className="size-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">89</div>
            <p className="text-xs text-muted-foreground">Certificates issued</p>
          </CardContent>
        </Card>
      </div>

      {isManager && (
        <div className="space-y-3">
          <h2 className="text-xl font-semibold flex items-center gap-2">
            <Flag className="size-5 text-amber-500" />
            {t('dashboard.readinessFlags')}
            {teamFlags.length > 0 && (
              <span className="text-sm font-normal text-muted-foreground">({teamFlags.length})</span>
            )}
          </h2>

          {teamFlags.length === 0 ? (
            <p className="text-muted-foreground text-sm">{t('dashboard.noFlags')}</p>
          ) : (
            <div className="rounded-md border">
              <table className="w-full">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="p-3 text-left font-medium">{t('dashboard.flagConsultant')}</th>
                    <th className="p-3 text-left font-medium">{t('dashboard.flagGoal')}</th>
                    <th className="p-3 text-left font-medium">{t('dashboard.flagAge')}</th>
                  </tr>
                </thead>
                <tbody>
                  {teamFlags.map((flag: TeamFlag) => (
                    <tr key={flag.goalId} className="border-b">
                      <td className="p-3 font-medium">{flag.consultantId}</td>
                      <td className="p-3">{flag.goalTitle}</td>
                      <td className="p-3">
                        <span
                          className={`text-sm ${daysAgo(flag.raisedAt) >= 7 ? 'text-red-600 font-medium' : 'text-amber-600'}`}
                        >
                          {t('dashboard.daysAgo', { days: daysAgo(flag.raisedAt) })}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
