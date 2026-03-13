import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button, Input } from '@itenium-forge/ui';
import {
  fetchUsers,
  fetchUserTeams,
  createUser,
  archiveUser,
  fetchArchivedUsers,
  restoreUser,
  fetchUncoachedUsers,
  type User,
  type ArchivedUser,
  type CreateUserRequest,
} from '@/api/client';

const ROLES = ['backoffice', 'manager', 'learner'] as const;

const emptyForm: CreateUserRequest = {
  firstName: '',
  lastName: '',
  email: '',
  role: 'learner',
  password: '',
  teamIds: [],
};

export function Users() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [showArchived, setShowArchived] = useState(false);
  const [showUncoached, setShowUncoached] = useState(false);
  const [form, setForm] = useState(emptyForm);

  const { data: users, isLoading } = useQuery<User[]>({
    queryKey: ['users'],
    queryFn: fetchUsers,
  });

  const { data: teams } = useQuery({
    queryKey: ['teams'],
    queryFn: fetchUserTeams,
  });

  const { data: archivedUsers } = useQuery<ArchivedUser[]>({
    queryKey: ['users', 'archived'],
    queryFn: fetchArchivedUsers,
    enabled: showArchived,
  });

  const { data: uncoachedUsers } = useQuery({
    queryKey: ['users', 'uncoached'],
    queryFn: fetchUncoachedUsers,
    enabled: showUncoached,
  });

  const createMutation = useMutation({
    mutationFn: createUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setShowForm(false);
      setForm(emptyForm);
    },
  });

  const archiveMutation = useMutation({
    mutationFn: archiveUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['users', 'archived'] });
      queryClient.invalidateQueries({ queryKey: ['users', 'uncoached'] });
    },
  });

  const restoreMutation = useMutation({
    mutationFn: restoreUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['users', 'archived'] });
      queryClient.invalidateQueries({ queryKey: ['users', 'uncoached'] });
    },
  });

  const handleTeamToggle = (teamId: number) => {
    setForm((f) => ({
      ...f,
      teamIds: f.teamIds.includes(teamId) ? f.teamIds.filter((id) => id !== teamId) : [...f.teamIds, teamId],
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(form);
  };

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">{t('users.title')}</h1>
        <Button onClick={() => setShowForm(!showForm)}>{t('users.addUser')}</Button>
      </div>

      {showForm && (
        <div className="rounded-md border p-4">
          <h2 className="text-lg font-semibold mb-4">{t('users.newUser')}</h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">{t('users.firstName')}</label>
                <Input
                  value={form.firstName}
                  onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">{t('users.lastName')}</label>
                <Input
                  value={form.lastName}
                  onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                  required
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('users.email')}</label>
              <Input
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('users.role')}</label>
              <select
                className="w-full rounded-md border px-3 py-2 text-sm"
                value={form.role}
                onChange={(e) => setForm({ ...form, role: e.target.value })}
              >
                {ROLES.map((role) => (
                  <option key={role} value={role}>
                    {t(`users.roles.${role}`)}
                  </option>
                ))}
              </select>
            </div>
            {teams && teams.length > 0 && (
              <div>
                <label className="block text-sm font-medium mb-1">{t('users.teams')}</label>
                <div className="flex flex-wrap gap-3">
                  {teams.map((team) => (
                    <label key={team.id} className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        checked={form.teamIds.includes(team.id)}
                        onChange={() => handleTeamToggle(team.id)}
                      />
                      {team.name}
                    </label>
                  ))}
                </div>
              </div>
            )}
            <div>
              <label className="block text-sm font-medium mb-1">{t('users.password')}</label>
              <Input
                type="password"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                required
              />
            </div>
            <div className="flex gap-2">
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending ? t('common.loading') : t('common.save')}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setShowForm(false);
                  setForm(emptyForm);
                }}
              >
                {t('common.cancel')}
              </Button>
            </div>
            {createMutation.isError && <p className="text-sm text-destructive">{t('common.error')}</p>}
          </form>
        </div>
      )}

      {/* Active Users Table */}
      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="p-3 text-left font-medium">{t('users.name')}</th>
              <th className="p-3 text-left font-medium">{t('users.email')}</th>
              <th className="p-3 text-left font-medium">{t('users.role')}</th>
              <th className="p-3 text-left font-medium">{t('users.teams')}</th>
              <th className="p-3 text-left font-medium"></th>
            </tr>
          </thead>
          <tbody>
            {users?.map((user) => (
              <tr key={user.id} className="border-b">
                <td className="p-3">
                  {user.firstName} {user.lastName}
                </td>
                <td className="p-3 text-muted-foreground">{user.email}</td>
                <td className="p-3">{t(`users.roles.${user.role}`) || user.role}</td>
                <td className="p-3 text-muted-foreground">
                  {user.teamIds.map((id) => teams?.find((t) => t.id === id)?.name ?? id).join(', ') || '-'}
                </td>
                <td className="p-3">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => archiveMutation.mutate(user.id)}
                    disabled={archiveMutation.isPending}
                  >
                    {t('users.archive')}
                  </Button>
                </td>
              </tr>
            ))}
            {users?.length === 0 && (
              <tr>
                <td colSpan={5} className="p-3 text-center text-muted-foreground">
                  {t('users.noUsers')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Archived Users Section */}
      <div>
        <Button variant="outline" onClick={() => setShowArchived(!showArchived)}>
          {showArchived ? t('users.hideArchived') : t('users.showArchived')}
        </Button>
        {showArchived && (
          <div className="mt-4 rounded-md border">
            <div className="p-3 bg-muted/50 font-medium border-b">{t('users.archivedUsers')}</div>
            <table className="w-full">
              <thead>
                <tr className="border-b bg-muted/30">
                  <th className="p-3 text-left font-medium">{t('users.name')}</th>
                  <th className="p-3 text-left font-medium">{t('users.email')}</th>
                  <th className="p-3 text-left font-medium">{t('users.role')}</th>
                  <th className="p-3 text-left font-medium">{t('users.archivedAt')}</th>
                  <th className="p-3 text-left font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {archivedUsers?.map((user) => (
                  <tr key={user.id} className="border-b opacity-70">
                    <td className="p-3">
                      {user.firstName} {user.lastName}
                    </td>
                    <td className="p-3 text-muted-foreground">{user.email}</td>
                    <td className="p-3">{t(`users.roles.${user.role}`) || user.role}</td>
                    <td className="p-3 text-muted-foreground">{new Date(user.archivedAt).toLocaleDateString()}</td>
                    <td className="p-3">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => restoreMutation.mutate(user.id)}
                        disabled={restoreMutation.isPending}
                      >
                        {t('users.restore')}
                      </Button>
                    </td>
                  </tr>
                ))}
                {archivedUsers?.length === 0 && (
                  <tr>
                    <td colSpan={5} className="p-3 text-center text-muted-foreground">
                      {t('users.noArchivedUsers')}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Uncoached Consultants Section (SF-33) */}
      <div>
        <Button variant="outline" onClick={() => setShowUncoached(!showUncoached)}>
          {showUncoached ? t('users.hideUncoached') : t('users.showUncoached')}
        </Button>
        {showUncoached && (
          <div className="mt-4 rounded-md border">
            <div className="p-3 bg-muted/50 font-medium border-b">{t('users.uncoachedUsers')}</div>
            <table className="w-full">
              <thead>
                <tr className="border-b bg-muted/30">
                  <th className="p-3 text-left font-medium">{t('users.name')}</th>
                  <th className="p-3 text-left font-medium">{t('users.email')}</th>
                  <th className="p-3 text-left font-medium">{t('users.teams')}</th>
                </tr>
              </thead>
              <tbody>
                {uncoachedUsers?.map((user) => (
                  <tr key={user.id} className="border-b">
                    <td className="p-3">
                      {user.firstName} {user.lastName}
                    </td>
                    <td className="p-3 text-muted-foreground">{user.email}</td>
                    <td className="p-3 text-muted-foreground">
                      {user.teamIds.map((id) => teams?.find((t) => t.id === id)?.name ?? id).join(', ') || '-'}
                    </td>
                  </tr>
                ))}
                {uncoachedUsers?.length === 0 && (
                  <tr>
                    <td colSpan={3} className="p-3 text-center text-muted-foreground">
                      {t('users.noUncoachedUsers')}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
