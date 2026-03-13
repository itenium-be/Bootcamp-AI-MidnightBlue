import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Loader2, CheckCircle2, ThumbsUp, ThumbsDown } from 'lucide-react';
import {
  Button,
  Input,
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
  Card,
  CardHeader,
  CardTitle,
  CardContent,
  CardFooter,
} from '@itenium-forge/ui';
import {
  fetchResources,
  fetchCourses,
  contributeResource,
  fetchMyCompletions,
  markResourceCompleted,
  removeCompletion,
  fetchMyRatings,
  rateResource,
  removeRating,
  type ContributeResourceRequest,
  type Resource,
  type ResourceRating,
  type ResourceType,
} from '@/api/client';

const RESOURCE_TYPES: ResourceType[] = ['Article', 'Video', 'Book', 'Course', 'Other'];

const createFormSchema = (t: (key: string) => string) =>
  z.object({
    title: z.string().min(1, t('resourceLibrary.form.titleRequired')).max(200),
    url: z.string().min(1, t('resourceLibrary.form.urlRequired')).url(t('resourceLibrary.form.urlInvalid')),
    type: z.string().min(1, t('resourceLibrary.form.typeRequired')),
    skillId: z.string().min(1, t('resourceLibrary.form.skillRequired')),
    fromLevel: z.string().optional(),
    toLevel: z.string().optional(),
    description: z.string().max(2000).optional(),
  });

type FormData = z.infer<ReturnType<typeof createFormSchema>>;

export function ResourceLibrary() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [skillFilter, setSkillFilter] = useState<number | undefined>();
  const [typeFilter, setTypeFilter] = useState<ResourceType | undefined>();

  const formSchema = createFormSchema(t);
  const form = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: { title: '', url: '', description: '', fromLevel: undefined, toLevel: undefined },
  });

  const { data: resources, isLoading } = useQuery({
    queryKey: ['resources', skillFilter, typeFilter],
    queryFn: () => fetchResources({ skillId: skillFilter, type: typeFilter }),
  });

  const { data: skills } = useQuery({
    queryKey: ['courses'],
    queryFn: fetchCourses,
  });

  const { data: completedIds = [] } = useQuery({
    queryKey: ['resource-completions'],
    queryFn: fetchMyCompletions,
  });

  const contributeMutation = useMutation({
    mutationFn: contributeResource,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resources'] });
      form.reset();
      setShowForm(false);
    },
  });

  const completeMutation = useMutation({
    mutationFn: markResourceCompleted,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resource-completions'] });
    },
  });

  const uncompleteMutation = useMutation({
    mutationFn: removeCompletion,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resource-completions'] });
    },
  });

  const { data: myRatings = [] } = useQuery<ResourceRating[]>({
    queryKey: ['resource-ratings'],
    queryFn: fetchMyRatings,
  });

  const myRatingsMap = new Map(myRatings.map((r) => [r.resourceId, r.isUpvote]));

  const rateMutation = useMutation({
    mutationFn: ({ resourceId, isUpvote }: { resourceId: number; isUpvote: boolean }) =>
      rateResource(resourceId, isUpvote),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resources'] });
      queryClient.invalidateQueries({ queryKey: ['resource-ratings'] });
    },
  });

  const removeRatingMutation = useMutation({
    mutationFn: removeRating,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resources'] });
      queryClient.invalidateQueries({ queryKey: ['resource-ratings'] });
    },
  });

  const onSubmit = (data: FormData) => {
    const request: ContributeResourceRequest = {
      title: data.title,
      url: data.url,
      type: data.type as ResourceType,
      skillId: parseInt(data.skillId),
      fromLevel: data.fromLevel ? parseInt(data.fromLevel) : null,
      toLevel: data.toLevel ? parseInt(data.toLevel) : null,
      description: data.description || null,
    };
    contributeMutation.mutate(request);
  };

  if (isLoading) {
    return <div>{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">{t('resourceLibrary.title')}</h1>
        <Button onClick={() => setShowForm((v) => !v)} variant={showForm ? 'outline' : 'default'}>
          <Plus className="size-4 mr-2" />
          {t('resourceLibrary.contribute')}
        </Button>
      </div>

      {showForm && (
        <Card>
          <CardHeader>
            <CardTitle>{t('resourceLibrary.form.title')}</CardTitle>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form id="contribute-form" onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="title"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t('resourceLibrary.form.titleLabel')}</FormLabel>
                        <FormControl>
                          <Input placeholder={t('resourceLibrary.form.titlePlaceholder')} {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="url"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t('resourceLibrary.form.urlLabel')}</FormLabel>
                        <FormControl>
                          <Input placeholder="https://" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="type"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t('resourceLibrary.form.typeLabel')}</FormLabel>
                        <FormControl>
                          <select className="w-full rounded-md border px-3 py-2 text-sm bg-background" {...field}>
                            <option value="">{t('resourceLibrary.form.typePlaceholder')}</option>
                            {RESOURCE_TYPES.map((type) => (
                              <option key={type} value={type}>
                                {t(`resourceLibrary.type.${type.toLowerCase()}`)}
                              </option>
                            ))}
                          </select>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="skillId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t('resourceLibrary.form.skillLabel')}</FormLabel>
                        <FormControl>
                          <select className="w-full rounded-md border px-3 py-2 text-sm bg-background" {...field}>
                            <option value="">{t('resourceLibrary.form.skillPlaceholder')}</option>
                            {skills?.map((s) => (
                              <option key={s.id} value={s.id}>
                                {s.name}
                              </option>
                            ))}
                          </select>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="fromLevel"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t('resourceLibrary.form.fromLevelLabel')}</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            min={1}
                            placeholder={t('resourceLibrary.form.levelPlaceholder')}
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="toLevel"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t('resourceLibrary.form.toLevelLabel')}</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            min={1}
                            placeholder={t('resourceLibrary.form.levelPlaceholder')}
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('resourceLibrary.form.descriptionLabel')}</FormLabel>
                      <FormControl>
                        <Input
                          placeholder={t('resourceLibrary.form.descriptionPlaceholder')}
                          {...field}
                          value={field.value ?? ''}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </form>
            </Form>
          </CardContent>
          <CardFooter className="flex gap-2 justify-end">
            <Button
              variant="outline"
              onClick={() => {
                setShowForm(false);
                form.reset();
              }}
            >
              {t('common.cancel')}
            </Button>
            <Button type="submit" form="contribute-form" disabled={contributeMutation.isPending}>
              {contributeMutation.isPending ? <Loader2 className="size-4 animate-spin mr-2" /> : null}
              {t('resourceLibrary.form.submit')}
            </Button>
          </CardFooter>
        </Card>
      )}

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
              <th className="p-3 text-left font-medium">{t('resourceLibrary.columnCompleted')}</th>
            </tr>
          </thead>
          <tbody>
            {resources?.map((resource: Resource) => (
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
                <td className="p-3">
                  <div className="flex items-center gap-2">
                    <Button
                      variant={myRatingsMap.get(resource.id) === true ? 'default' : 'outline'}
                      size="sm"
                      disabled={rateMutation.isPending || removeRatingMutation.isPending}
                      onClick={() =>
                        myRatingsMap.get(resource.id) === true
                          ? removeRatingMutation.mutate(resource.id)
                          : rateMutation.mutate({ resourceId: resource.id, isUpvote: true })
                      }
                      aria-label={t('resourceLibrary.rateUp')}
                    >
                      <ThumbsUp className="size-3 mr-1" />
                      {resource.upvotes}
                    </Button>
                    <Button
                      variant={myRatingsMap.get(resource.id) === false ? 'default' : 'outline'}
                      size="sm"
                      disabled={rateMutation.isPending || removeRatingMutation.isPending}
                      onClick={() =>
                        myRatingsMap.get(resource.id) === false
                          ? removeRatingMutation.mutate(resource.id)
                          : rateMutation.mutate({ resourceId: resource.id, isUpvote: false })
                      }
                      aria-label={t('resourceLibrary.rateDown')}
                    >
                      <ThumbsDown className="size-3 mr-1" />
                      {resource.downvotes}
                    </Button>
                  </div>
                </td>
                <td className="p-3">
                  {completedIds.includes(resource.id) ? (
                    <Button
                      variant="default"
                      size="sm"
                      disabled={uncompleteMutation.isPending}
                      onClick={() => uncompleteMutation.mutate(resource.id)}
                      className="text-green-600 gap-1"
                    >
                      <CheckCircle2 className="size-4" />
                      {t('resourceLibrary.completed')}
                    </Button>
                  ) : (
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={completeMutation.isPending}
                      onClick={() => completeMutation.mutate(resource.id)}
                    >
                      {t('resourceLibrary.markCompleted')}
                    </Button>
                  )}
                </td>
              </tr>
            ))}
            {resources?.length === 0 && (
              <tr>
                <td colSpan={6} className="p-3 text-center text-muted-foreground">
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
