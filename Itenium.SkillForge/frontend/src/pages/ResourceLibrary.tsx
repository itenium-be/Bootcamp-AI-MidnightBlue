import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@itenium-forge/ui';
import { ThumbsUp, ThumbsDown, CheckCircle, Circle, Plus, ExternalLink } from 'lucide-react';
import {
  fetchResources,
  createResource,
  markResourceComplete,
  unmarkResourceComplete,
  rateResource,
  removeResourceRating,
  fetchSkillCatalogue,
  type Resource,
} from '@/api/client';

const TYPE_COLORS: Record<string, string> = {
  article: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  video: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
  book: 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200',
  course: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  podcast: 'bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-200',
  other: 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200',
};

function TypeBadge({ type }: { type: string }) {
  const { t } = useTranslation();
  const colorClass = TYPE_COLORS[type] ?? TYPE_COLORS['other'];
  const label = t(`resources.${type}`, { defaultValue: type });
  return (
    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${colorClass}`}>
      {label}
    </span>
  );
}

interface ResourceRowProps {
  resource: Resource;
  onToggleComplete: (resource: Resource) => void;
  onRate: (resource: Resource, isPositive: boolean) => void;
  onRemoveRating: (resource: Resource) => void;
}

function ResourceRow({ resource, onToggleComplete, onRate, onRemoveRating }: ResourceRowProps) {
  const { t } = useTranslation();

  const levelRange =
    resource.fromLevel != null || resource.toLevel != null
      ? `${resource.fromLevel ?? '?'} → ${resource.toLevel ?? '?'}`
      : null;

  return (
    <tr className="border-b hover:bg-muted/40">
      <td className="p-3">
        <a
          href={resource.url}
          target="_blank"
          rel="noopener noreferrer"
          className="font-medium text-primary hover:underline flex items-center gap-1"
        >
          {resource.title}
          <ExternalLink className="size-3 opacity-60" />
        </a>
      </td>
      <td className="p-3">
        <TypeBadge type={resource.type} />
      </td>
      <td className="p-3 text-sm text-muted-foreground">
        {resource.skillName ? (
          <span>
            {resource.skillName}
            {levelRange && <span className="ml-1 opacity-70">({levelRange})</span>}
          </span>
        ) : (
          '-'
        )}
      </td>
      <td className="p-3 text-center">
        <button
          onClick={() => onToggleComplete(resource)}
          title={resource.completedByCurrentUser ? t('resources.completed') : t('resources.completed')}
          className="text-muted-foreground hover:text-primary transition-colors"
        >
          {resource.completedByCurrentUser ? (
            <CheckCircle className="size-5 text-green-500" />
          ) : (
            <Circle className="size-5" />
          )}
        </button>
      </td>
      <td className="p-3">
        <div className="flex items-center gap-2">
          <button
            onClick={() => (resource.currentUserRating === true ? onRemoveRating(resource) : onRate(resource, true))}
            className={`flex items-center gap-1 text-sm transition-colors ${
              resource.currentUserRating === true
                ? 'text-green-600 dark:text-green-400 font-semibold'
                : 'text-muted-foreground hover:text-green-600'
            }`}
          >
            <ThumbsUp className="size-4" />
            <span>{resource.thumbsUp}</span>
          </button>
          <button
            onClick={() => (resource.currentUserRating === false ? onRemoveRating(resource) : onRate(resource, false))}
            className={`flex items-center gap-1 text-sm transition-colors ${
              resource.currentUserRating === false
                ? 'text-red-600 dark:text-red-400 font-semibold'
                : 'text-muted-foreground hover:text-red-600'
            }`}
          >
            <ThumbsDown className="size-4" />
            <span>{resource.thumbsDown}</span>
          </button>
        </div>
      </td>
    </tr>
  );
}

export function ResourceLibrary() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showAddForm, setShowAddForm] = useState(false);

  // Form state
  const [formTitle, setFormTitle] = useState('');
  const [formUrl, setFormUrl] = useState('');
  const [formType, setFormType] = useState('article');
  const [formSkillId, setFormSkillId] = useState<number | null>(null);
  const [formFromLevel, setFormFromLevel] = useState<number | null>(null);
  const [formToLevel, setFormToLevel] = useState<number | null>(null);

  const { data: resources, isLoading } = useQuery({
    queryKey: ['resources'],
    queryFn: fetchResources,
  });

  const { data: catalogue } = useQuery({
    queryKey: ['skills', 'catalogue'],
    queryFn: fetchSkillCatalogue,
  });

  const skills = catalogue?.flatMap((c) => c.skills) ?? [];

  const invalidateResources = () => queryClient.invalidateQueries({ queryKey: ['resources'] });

  const createMutation = useMutation({
    mutationFn: createResource,
    onSuccess: () => {
      invalidateResources();
      setShowAddForm(false);
      setFormTitle('');
      setFormUrl('');
      setFormType('article');
      setFormSkillId(null);
      setFormFromLevel(null);
      setFormToLevel(null);
    },
  });

  const completeMutation = useMutation({
    mutationFn: markResourceComplete,
    onSuccess: invalidateResources,
  });

  const uncompleteMutation = useMutation({
    mutationFn: unmarkResourceComplete,
    onSuccess: invalidateResources,
  });

  const rateMutation = useMutation({
    mutationFn: ({ id, isPositive }: { id: number; isPositive: boolean }) => rateResource(id, isPositive),
    onSuccess: invalidateResources,
  });

  const removeRatingMutation = useMutation({
    mutationFn: removeResourceRating,
    onSuccess: invalidateResources,
  });

  const handleToggleComplete = (resource: Resource) => {
    if (resource.completedByCurrentUser) {
      uncompleteMutation.mutate(resource.id);
    } else {
      completeMutation.mutate(resource.id);
    }
  };

  const handleRate = (resource: Resource, isPositive: boolean) => {
    rateMutation.mutate({ id: resource.id, isPositive });
  };

  const handleRemoveRating = (resource: Resource) => {
    removeRatingMutation.mutate(resource.id);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate({
      title: formTitle,
      url: formUrl,
      type: formType,
      skillId: formSkillId,
      fromLevel: formFromLevel,
      toLevel: formToLevel,
    });
  };

  const resourceTypes = ['article', 'video', 'book', 'course', 'podcast', 'other'];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('resources.title')}</h1>
          <p className="text-muted-foreground mt-1">{t('resources.subtitle')}</p>
        </div>
        <Button onClick={() => setShowAddForm((v) => !v)} size="sm">
          <Plus className="size-4 mr-1" />
          {t('resources.addResource')}
        </Button>
      </div>

      {showAddForm && (
        <div className="rounded-md border p-4 bg-muted/20 space-y-4">
          <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium">{t('resources.title_field')}</label>
              <input
                required
                className="border rounded px-3 py-1.5 text-sm bg-background"
                value={formTitle}
                onChange={(e) => setFormTitle(e.target.value)}
                placeholder={t('resources.title_field')}
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium">{t('resources.url')}</label>
              <input
                required
                type="url"
                className="border rounded px-3 py-1.5 text-sm bg-background"
                value={formUrl}
                onChange={(e) => setFormUrl(e.target.value)}
                placeholder="https://..."
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium">{t('resources.type')}</label>
              <select
                className="border rounded px-3 py-1.5 text-sm bg-background"
                value={formType}
                onChange={(e) => setFormType(e.target.value)}
              >
                {resourceTypes.map((type) => (
                  <option key={type} value={type}>
                    {t(`resources.${type}`)}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium">{t('resources.skill')}</label>
              <select
                className="border rounded px-3 py-1.5 text-sm bg-background"
                value={formSkillId ?? ''}
                onChange={(e) => setFormSkillId(e.target.value ? Number(e.target.value) : null)}
              >
                <option value="">{t('resources.noSkill')}</option>
                {skills.map((skill) => (
                  <option key={skill.id} value={skill.id}>
                    {skill.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium">{t('resources.fromLevel')}</label>
              <input
                type="number"
                min="1"
                className="border rounded px-3 py-1.5 text-sm bg-background"
                value={formFromLevel ?? ''}
                onChange={(e) => setFormFromLevel(e.target.value ? Number(e.target.value) : null)}
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium">{t('resources.toLevel')}</label>
              <input
                type="number"
                min="1"
                className="border rounded px-3 py-1.5 text-sm bg-background"
                value={formToLevel ?? ''}
                onChange={(e) => setFormToLevel(e.target.value ? Number(e.target.value) : null)}
              />
            </div>
            <div className="flex items-end gap-2 sm:col-span-2 lg:col-span-3">
              <Button type="submit" size="sm" disabled={createMutation.isPending}>
                {t('resources.addResource')}
              </Button>
              <Button type="button" variant="ghost" size="sm" onClick={() => setShowAddForm(false)}>
                {t('resources.cancel')}
              </Button>
            </div>
          </form>
        </div>
      )}

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('resources.title_field')}</th>
              <th className="p-3 text-left font-medium">{t('resources.type')}</th>
              <th className="p-3 text-left font-medium">{t('resources.skill')}</th>
              <th className="p-3 text-center font-medium">{t('resources.completed')}</th>
              <th className="p-3 text-left font-medium"></th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={5} className="p-4 text-center text-muted-foreground">
                  {t('common.loading')}
                </td>
              </tr>
            )}
            {!isLoading && (!resources || resources.length === 0) && (
              <tr>
                <td colSpan={5} className="p-4 text-center text-muted-foreground">
                  {t('resources.noResources')}
                </td>
              </tr>
            )}
            {!isLoading &&
              resources?.map((resource) => (
                <ResourceRow
                  key={resource.id}
                  resource={resource}
                  onToggleComplete={handleToggleComplete}
                  onRate={handleRate}
                  onRemoveRating={handleRemoveRating}
                />
              ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
