import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Badge, Button } from '@itenium-forge/ui';
import { ChevronDown, ChevronRight, AlertTriangle } from 'lucide-react';
import {
  fetchSkillCatalogue,
  fetchProfiles,
  fetchProfileSkills,
  type SkillCategory,
  type SkillDetail,
  type SkillLevelDescriptor,
  type SkillSummary,
  type CompetenceCentreProfile,
} from '@/api/client';

function LevelBadge({ levelCount }: { levelCount: number }) {
  const { t } = useTranslation();
  if (levelCount === 1) {
    return <Badge variant="secondary">{t('skills.checkbox')}</Badge>;
  }
  return <Badge variant="outline">{t('skills.levels', { count: levelCount })}</Badge>;
}

function SkillRow({ skill }: { skill: SkillSummary }) {
  const [open, setOpen] = useState(false);
  const { t } = useTranslation();

  const { data: detail, isLoading } = useQuery<SkillDetail>({
    queryKey: ['skill', skill.id],
    queryFn: async () => {
      const { fetchSkill } = await import('@/api/client');
      return fetchSkill(skill.id);
    },
    enabled: open,
  });

  return (
    <>
      <tr className="border-b hover:bg-muted/40 cursor-pointer" onClick={() => setOpen((v) => !v)}>
        <td className="p-3 w-6 text-muted-foreground">
          {open ? <ChevronDown className="size-4" /> : <ChevronRight className="size-4" />}
        </td>
        <td className="p-3 font-medium">{skill.name}</td>
        <td className="p-3 text-muted-foreground text-sm">{skill.description || '-'}</td>
        <td className="p-3">
          <LevelBadge levelCount={skill.levelCount} />
        </td>
      </tr>
      {open && (
        <tr className="border-b bg-muted/20">
          <td />
          <td colSpan={3} className="p-4 space-y-3">
            {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
            {detail && (
              <>
                {detail.prerequisites.length > 0 && (
                  <div className="flex items-start gap-2 text-sm text-amber-600 dark:text-amber-400">
                    <AlertTriangle className="size-4 mt-0.5 shrink-0" />
                    <span>
                      {t('skills.prerequisites')}:{' '}
                      {detail.prerequisites
                        .map((p) => `${p.requiredSkillName} ${t('skills.niveau')} ${p.requiredLevel}`)
                        .join(', ')}
                    </span>
                  </div>
                )}
                {detail.levelDescriptors.length > 0 && (
                  <ol className="space-y-1 text-sm list-none">
                    {detail.levelDescriptors.map((d: SkillLevelDescriptor) => (
                      <li key={d.level} className="flex gap-2">
                        <span className="font-semibold text-primary w-6 shrink-0">
                          {detail.levelCount === 1 ? '✓' : d.level}
                        </span>
                        <span className="text-muted-foreground">{d.description}</span>
                      </li>
                    ))}
                  </ol>
                )}
              </>
            )}
          </td>
        </tr>
      )}
    </>
  );
}

function CategorySection({ category }: { category: SkillCategory }) {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <>
      <tr className="bg-muted/50 cursor-pointer select-none" onClick={() => setCollapsed((v) => !v)}>
        <td className="p-3 w-6">
          {collapsed ? <ChevronRight className="size-4" /> : <ChevronDown className="size-4" />}
        </td>
        <td colSpan={3} className="p-3 font-semibold text-sm uppercase tracking-wide">
          {category.category}
          <span className="ml-2 font-normal text-muted-foreground normal-case tracking-normal">
            ({category.skills.length})
          </span>
        </td>
      </tr>
      {!collapsed && category.skills.map((skill) => <SkillRow key={skill.id} skill={skill} />)}
    </>
  );
}

export function SkillCatalogue() {
  const { t } = useTranslation();
  const [selectedProfileId, setSelectedProfileId] = useState<number | null>(null);

  const { data: catalogue, isLoading: catalogueLoading } = useQuery({
    queryKey: ['skills', 'catalogue'],
    queryFn: fetchSkillCatalogue,
    enabled: selectedProfileId === null,
  });

  const { data: profiles, isLoading: profilesLoading } = useQuery({
    queryKey: ['profiles'],
    queryFn: fetchProfiles,
  });

  const { data: profileSkills, isLoading: profileSkillsLoading } = useQuery({
    queryKey: ['profile', selectedProfileId, 'skills'],
    queryFn: () => fetchProfileSkills(selectedProfileId ?? 0),
    enabled: selectedProfileId !== null,
  });

  const isLoading = selectedProfileId === null ? catalogueLoading : profileSkillsLoading;

  // Group profile skills by category when a profile is selected
  const profileCategories: SkillCategory[] = profileSkills
    ? Object.entries(
        profileSkills.reduce<Record<string, SkillSummary[]>>((acc, skill) => {
          if (!acc[skill.category]) {
            acc[skill.category] = [];
          }
          acc[skill.category].push(skill);
          return acc;
        }, {}),
      )
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([category, skills]) => ({ category, skills }))
    : [];

  const displayedCategories = selectedProfileId === null ? (catalogue ?? []) : profileCategories;

  const selectedProfile: CompetenceCentreProfile | undefined = profiles?.find((p) => p.id === selectedProfileId);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('skills.title')}</h1>
        <p className="text-muted-foreground mt-1">{t('skills.subtitle')}</p>
      </div>

      {/* Profile filter */}
      <div className="flex flex-wrap gap-2 items-center">
        <span className="text-sm font-medium text-muted-foreground">{t('skills.filterByProfile')}:</span>
        <Button
          variant={selectedProfileId === null ? 'default' : 'outline'}
          size="sm"
          onClick={() => setSelectedProfileId(null)}
        >
          {t('skills.allSkills')}
        </Button>
        {profilesLoading && <span className="text-sm text-muted-foreground">{t('common.loading')}</span>}
        {profiles?.map((profile) => (
          <Button
            key={profile.id}
            variant={selectedProfileId === profile.id ? 'default' : 'outline'}
            size="sm"
            onClick={() => setSelectedProfileId(profile.id)}
          >
            {profile.name}
            <span className="ml-1 text-xs opacity-70">({profile.skillCount})</span>
          </Button>
        ))}
      </div>

      {selectedProfile && <p className="text-sm text-muted-foreground">{selectedProfile.description}</p>}

      {/* Skill table */}
      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 w-6" />
              <th className="p-3 text-left font-medium">{t('skills.name')}</th>
              <th className="p-3 text-left font-medium">{t('skills.description')}</th>
              <th className="p-3 text-left font-medium">{t('skills.levels')}</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={4} className="p-4 text-center text-muted-foreground">
                  {t('common.loading')}
                </td>
              </tr>
            )}
            {!isLoading && displayedCategories.length === 0 && (
              <tr>
                <td colSpan={4} className="p-4 text-center text-muted-foreground">
                  {t('skills.noSkills')}
                </td>
              </tr>
            )}
            {!isLoading && displayedCategories.map((cat) => <CategorySection key={cat.category} category={cat} />)}
          </tbody>
        </table>
      </div>
    </div>
  );
}
