import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { fetchSkills } from '@/api/client';

export function Courses() {
  const { t } = useTranslation();

  const { data: skills, isLoading } = useQuery({
    queryKey: ['skills'],
    queryFn: fetchSkills,
  });

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('skills.title')}</h1>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('skills.name')}</th>
              <th className="p-3 text-left font-medium">{t('skills.description')}</th>
              <th className="p-3 text-left font-medium">{t('skills.category')}</th>
              <th className="p-3 text-left font-medium">{t('skills.levelCount')}</th>
            </tr>
          </thead>
          <tbody>
            {skills?.map((skill) => (
              <tr key={skill.id} className="border-b">
                <td className="p-3">{skill.name}</td>
                <td className="p-3 text-muted-foreground">{skill.description || '-'}</td>
                <td className="p-3">{skill.category || '-'}</td>
                <td className="p-3">
                  {skill.levelCount === 1
                    ? t('skills.checkbox')
                    : t('skills.levels', { count: skill.levelCount })}
                </td>
              </tr>
            ))}
            {skills?.length === 0 && (
              <tr>
                <td colSpan={4} className="p-3 text-center text-muted-foreground">
                  {t('skills.noSkills')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
