import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Flag, CheckCircle2 } from 'lucide-react';
import { Button, Card, CardHeader, CardTitle, CardContent } from '@itenium-forge/ui';
import { fetchMyGoals, raiseReadinessFlag, type Goal } from '@/api/client';

function daysAgo(dateStr: string): number {
  return Math.floor((Date.now() - new Date(dateStr).getTime()) / 86_400_000);
}

export function MyGoals() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: goals = [], isLoading } = useQuery<Goal[]>({
    queryKey: ['my-goals'],
    queryFn: fetchMyGoals,
  });

  const flagMutation = useMutation({
    mutationFn: raiseReadinessFlag,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-goals'] });
    },
  });

  if (isLoading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">{t('goals.title')}</h1>

      {goals.length === 0 && <p className="text-muted-foreground">{t('goals.noGoals')}</p>}

      <div className="space-y-4">
        {goals.map((goal: Goal) => (
          <Card key={goal.id}>
            <CardHeader className="pb-2">
              <CardTitle className="text-base">{goal.title}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
                {goal.targetLevel != null && (
                  <span>
                    {t('goals.targetLevel')}: {goal.targetLevel}
                  </span>
                )}
                {goal.deadline && (
                  <span>
                    {t('goals.deadline')}: {new Date(goal.deadline).toLocaleDateString()}
                  </span>
                )}
                {goal.createdBy && (
                  <span>
                    {t('goals.setBy')}: {goal.createdBy}
                  </span>
                )}
              </div>

              {goal.notes && <p className="text-sm text-muted-foreground italic">{goal.notes}</p>}

              <div>
                {goal.flagRaisedAt ? (
                  <span className="flex items-center gap-1.5 text-sm text-amber-600">
                    <CheckCircle2 className="size-4" />
                    {t('goals.flagRaised', { days: daysAgo(goal.flagRaisedAt) })}
                  </span>
                ) : (
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={flagMutation.isPending}
                    onClick={() => flagMutation.mutate(goal.id)}
                  >
                    <Flag className="size-4 mr-2" />
                    {t('goals.raiseFlag')}
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
