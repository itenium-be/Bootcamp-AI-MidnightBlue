import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi } from 'vitest';
import { useTeamStore } from '@/stores/teamStore';
// eslint-disable-next-line import-x/order -- must come after vi.mock calls
import { Dashboard } from '../Dashboard';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock('@itenium-forge/ui', () => {
  const S = ({ children }: { children?: React.ReactNode }) => <div>{children}</div>;
  return { Card: S, CardHeader: S, CardTitle: S, CardContent: S };
});

vi.mock('lucide-react', () => ({
  BookOpen: () => <span />,
  Users: () => <span />,
  Award: () => <span />,
}));

beforeEach(() => {
  useTeamStore.setState({ mode: 'backoffice', selectedTeam: null, teams: [] });
});

describe('Dashboard', () => {
  it('shows the dashboard title', () => {
    render(<Dashboard />);
    expect(screen.getByText('dashboard.title')).toBeInTheDocument();
  });

  it('shows static stat: 24 total courses', () => {
    render(<Dashboard />);
    expect(screen.getByText('24')).toBeInTheDocument();
  });

  it('shows static stat: 156 active learners', () => {
    render(<Dashboard />);
    expect(screen.getByText('156')).toBeInTheDocument();
  });

  it('shows static stat: 89 completed courses', () => {
    render(<Dashboard />);
    expect(screen.getByText('89')).toBeInTheDocument();
  });

  it('appends team name to welcome message in manager mode', () => {
    useTeamStore.setState({ mode: 'manager', selectedTeam: { id: 1, name: 'Java Team' }, teams: [] });
    render(<Dashboard />);
    expect(screen.getByText(/Java Team/)).toBeInTheDocument();
  });

  it('does not append team name in backoffice mode', () => {
    useTeamStore.setState({ mode: 'backoffice', selectedTeam: null, teams: [] });
    render(<Dashboard />);
    expect(screen.queryByText(/ - /)).not.toBeInTheDocument();
  });

  it('does not append team name when manager has no selected team', () => {
    useTeamStore.setState({ mode: 'manager', selectedTeam: null, teams: [] });
    render(<Dashboard />);
    expect(screen.queryByText(/ - /)).not.toBeInTheDocument();
  });
});
