import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchResources, fetchCourses, type ResourceType } from '@/api/client';

const RESOURCE_TYPES: ResourceType[] = ['Article', 'Video', 'Book', 'Course', 'Other'];

export function ResourceLibrary() {
  const { t } = useTranslation();
  const [skillFilter, setSkillFilter] = useState<number | undefined>();
  const [typeFilter, setTypeFilter] = useState<ResourceType | undefined>();

  const { data: resources, isLoading } = useQuery({
    queryKey: ['resources', skillFilter, typeFilter],
    queryFn: () => fetchResources({ skillId: skillFilter, type: typeFilter }),
  });

  const { data: skills } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('resourceLibrary.title')}</h1>
      </div>

      <div className="flex gap-4">
        <select
          className="rounded-md border px-3 py-2 text-sm bg-background"
          value={skillFilter ?? ''}
          onChange={(e) => setSkillFilter(e.target.value ? Number(e.target.value) : undefined)}
        >
          <option value="">{t('resourceLibrary.allSkills')}</option>
          {skills?.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </select>

        <select
          className="rounded-md border px-3 py-2 text-sm bg-background"
          value={typeFilter ?? ''}
          onChange={(e) => setTypeFilter((e.target.value || undefined) as ResourceType | undefined)}
        >
          <option value="">{t('resourceLibrary.allTypes')}</option>
          {RESOURCE_TYPES.map((type) => (
            <option key={type} value={type}>
              {t(`resourceLibrary.type.${type.toLowerCase()}`)}
            </option>
          ))}
        </select>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('resourceLibrary.columnTitle')}</th>
              <th className="p-3 text-left font-medium">{t('resourceLibrary.columnType')}</th>
              <th className="p-3 text-left font-medium">{t('resourceLibrary.columnSkill')}</th>
              <th className="p-3 text-left font-medium">{t('resourceLibrary.columnNiveau')}</th>
              <th className="p-3 text-left font-medium">{t('resourceLibrary.columnRating')}</th>
            </tr>
          </thead>
          <tbody>
            {resources?.map((resource) => (
              <tr key={resource.id} className="border-b">
                <td className="p-3">
                  <a
                    href={resource.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="hover:underline text-primary"
                  >
                    {resource.title}
                  </a>
                  {resource.description && (
                    <p className="text-xs text-muted-foreground mt-0.5">{resource.description}</p>
                  )}
                </td>
                <td className="p-3">{t(`resourceLibrary.type.${resource.type.toLowerCase()}`)}</td>
                <td className="p-3 text-muted-foreground">
                  {skills?.find((s) => s.id === resource.skillId)?.name ?? '-'}
                </td>
                <td className="p-3 text-muted-foreground">
                  {resource.fromLevel != null || resource.toLevel != null
                    ? `${resource.fromLevel ?? '?'} – ${resource.toLevel ?? '?'}`
                    : '-'}
                </td>
                <td className="p-3 text-muted-foreground">
                  👍 {resource.upvotes} / 👎 {resource.downvotes}
                </td>
              </tr>
            ))}
            {resources?.length === 0 && (
              <tr>
                <td colSpan={5} className="p-3 text-center text-muted-foreground">
                  {t('resourceLibrary.noResources')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
