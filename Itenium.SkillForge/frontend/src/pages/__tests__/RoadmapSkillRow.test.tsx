import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi } from 'vitest';
import { RoadmapSkillRow } from '../ConsultantProfile';
import type { RoadmapSkill } from '@/api/client';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, opts?: Record<string, unknown>) => {
      if (key === 'skills.levels') return `${String(opts?.count)} levels`;
      if (key === 'skills.checkbox') return 'Checkbox';
      if (key === 'consultant.prerequisiteWarning') return 'Prerequisites not yet met';
      if (key === 'skills.niveau') return 'niveau';
      return key;
    },
  }),
}));

vi.mock('@itenium-forge/ui', () => ({
  Badge: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
}));

vi.mock('lucide-react', () => ({
  ChevronRight: () => <span data-testid="chevron" />,
  AlertTriangle: () => <span data-testid="alert-triangle" />,
}));

const baseSkill: RoadmapSkill = {
  id: 1,
  name: 'Clean Code',
  category: 'Craftsmanship',
  description: null,
  levelCount: 3,
  unmetPrerequisites: [],
};

describe('RoadmapSkillRow', () => {
  it('renders the skill name', () => {
    render(
      <ul>
        <RoadmapSkillRow skill={baseSkill} />
      </ul>,
    );
    expect(screen.getByText('Clean Code')).toBeInTheDocument();
  });

  it('does not show warning when prerequisites are met', () => {
    render(
      <ul>
        <RoadmapSkillRow skill={baseSkill} />
      </ul>,
    );
    expect(screen.queryByTestId('alert-triangle')).not.toBeInTheDocument();
    expect(screen.queryByText(/Prerequisites not yet met/)).not.toBeInTheDocument();
  });

  it('shows warning when prerequisites are not met', () => {
    const skill: RoadmapSkill = {
      ...baseSkill,
      name: 'Domain-Driven Design',
      unmetPrerequisites: [{ requiredSkillId: 1, requiredSkillName: 'Clean Code', requiredLevel: 3 }],
    };
    render(
      <ul>
        <RoadmapSkillRow skill={skill} />
      </ul>,
    );
    expect(screen.getByTestId('alert-triangle')).toBeInTheDocument();
    expect(screen.getByText(/Prerequisites not yet met/)).toBeInTheDocument();
    expect(screen.getByText(/Clean Code niveau 3/)).toBeInTheDocument();
  });

  it('lists all unmet prerequisites in the warning', () => {
    const skill: RoadmapSkill = {
      ...baseSkill,
      name: 'Event Sourcing',
      unmetPrerequisites: [
        { requiredSkillId: 1, requiredSkillName: 'Clean Code', requiredLevel: 3 },
        { requiredSkillId: 2, requiredSkillName: 'Domain-Driven Design', requiredLevel: 2 },
      ],
    };
    render(
      <ul>
        <RoadmapSkillRow skill={skill} />
      </ul>,
    );
    expect(screen.getByText(/Clean Code niveau 3/)).toBeInTheDocument();
    expect(screen.getByText(/Domain-Driven Design niveau 2/)).toBeInTheDocument();
  });

  it('shows checkbox badge for level-1 skills', () => {
    const skill: RoadmapSkill = { ...baseSkill, levelCount: 1 };
    render(
      <ul>
        <RoadmapSkillRow skill={skill} />
      </ul>,
    );
    expect(screen.getByText('Checkbox')).toBeInTheDocument();
  });

  it('shows level count badge for multi-level skills', () => {
    render(
      <ul>
        <RoadmapSkillRow skill={baseSkill} />
      </ul>,
    );
    expect(screen.getByText('3 levels')).toBeInTheDocument();
  });
});
