# SkillForge — User Stories (MVP)

**Generated:** 2026-03-13
**Source:** PRD (prd.md), existing codebase analysis
**Scope:** MVP only — March 13, 2026 bootcamp demo

> **Codebase baseline:** Auth (OpenIddict/JWT), RBAC (backoffice/manager/learner), Teams, basic CourseEntity scaffold are already operational. Stories below cover what remains.

---

## Epic 1: Skill Catalogue

### SF-01 — Extend skill model with niveau system

**As a** platform admin,
**I want** skills to have a variable number of levels (1=checkbox, 2–7=progression) with a descriptor per niveau,
**so that** skill progression is meaningful and coaches can validate against defined criteria.

**Acceptance Criteria:**
- `SkillEntity` has: `name`, `description`, `category`, `levelCount` (1–7), `levelDescriptors[]` (one per niveau)
- A skill with `levelCount = 1` behaves as a checkbox (done / not done)
- A skill with `levelCount > 1` displays niveau labels and descriptors
- `CourseEntity` is renamed/migrated to `SkillEntity` (existing data migration included)

**Dependencies:** none
**FRs:** FR6, FR7

---

### SF-02 — Prerequisite links between skills

**As a** consultant,
**I want** to see when a skill has prerequisites I haven't met,
**so that** I understand the recommended learning order without being blocked.

**Acceptance Criteria:**
- Each skill can reference zero or more prerequisite skills with a minimum required niveau
- API returns prerequisite status per skill for the current consultant (met/unmet)
- Prerequisite check is warn-only — skill is never locked
- Warning text: *"[Skill X] niveau [N] not yet met — you can explore this skill, but your coach may ask you to address prerequisites first."*

**Dependencies:** SF-01
**FRs:** FR7, FR8

---

### SF-03 — Seed skill catalogue from itenium matrices

**As a** platform admin,
**I want** the skill catalogue pre-loaded from itenium's existing competence matrices,
**so that** the platform is ready to use from day one without manual data entry.

**Acceptance Criteria:**
- Seed script imports skills from itenium Excel matrices (Java, .NET, PO&Analysis, QA)
- Each imported skill includes: name, category, description, levelCount, level descriptors, and prerequisite links where applicable
- Seeding is idempotent (safe to re-run without duplicates)
- Seed data covers the full bootcamp demo script

**Dependencies:** SF-01, SF-02
**FRs:** FR9

---

### SF-04 — Two-layer skill architecture: profiles

**As a** coach,
**I want** to define competence centre profiles that filter a relevant subset of the global skill catalogue,
**so that** each consultant sees only the skills relevant to their discipline.

**Acceptance Criteria:**
- `ProfileEntity` exists with: name (Java, .NET, PO&Analysis, QA), and a list of included skill IDs
- Admin/backoffice can view available profiles
- Four default profiles seeded: Java, .NET, PO&Analysis, QA
- API endpoint returns the skill list for a given profile

**Dependencies:** SF-01, SF-03
**FRs:** FR10

---

## Epic 2: Consultant Profile & Roadmap

### SF-05 — Assign consultant to competence centre profile

**As a** coach,
**I want** to assign a consultant to a competence centre profile,
**so that** their roadmap is filtered to the skills relevant to their role.

**Acceptance Criteria:**
- Coach can assign any consultant on their team to one of the four profiles
- Assignment is visible on the consultant's profile
- Changing the profile updates the roadmap immediately
- A consultant without a profile assigned sees an empty roadmap with a prompt to their coach

**Dependencies:** SF-04
**FRs:** FR11

---

### SF-06 — Consultant roadmap — progressive disclosure

**As a** consultant,
**I want** to see my personalised roadmap showing my current skill levels and immediate next steps,
**so that** I always know where I stand and what to focus on next.

**Acceptance Criteria:**
- Default view shows 8–12 skill nodes: current anchors (skills with a validated niveau > 0) + immediate next-tier skills
- Each node displays: skill name, current niveau, target niveau (from active goal if set), completion state
- Skills with active goals are visually highlighted
- Page load <2s

**Dependencies:** SF-05, SF-01
**FRs:** FR12, FR13

---

### SF-07 — Expand roadmap to show all skills

**As a** consultant,
**I want** to expand my roadmap to see all skills in my profile,
**so that** I can explore the full landscape of my discipline.

**Acceptance Criteria:**
- "Show all" button/toggle reveals all skills in the profile (not just next-tier)
- Skills outside the default set are visually distinct (e.g., greyed out or in a secondary section)
- Dependency warnings are shown on expand (SF-02)
- Toggle state is not persisted — defaults to progressive view on next load

**Dependencies:** SF-06, SF-02
**FRs:** FR14

---

### SF-08 — Pre-populated first-login experience

**As a** consultant logging in for the first time,
**I want** to see my roadmap already populated with coach-assigned goals,
**so that** I'm not greeted by an empty screen.

**Acceptance Criteria:**
- If a consultant has ≥1 coach-assigned goal, the roadmap is shown with those goals highlighted
- Welcome message displays: *"Welcome, [Name]. Your coach has set [N] goals for you."*
- If no goals are set yet, a friendly placeholder is shown: *"Your coach will set your first goals soon."*

**Dependencies:** SF-06, SF-10 (Goals epic)
**FRs:** FR15

---

### SF-09 — Skill dependency warning (non-blocking)

**As a** consultant,
**I want** to see a warning when I view a skill whose prerequisites I haven't met,
**so that** I'm informed about the recommended learning path without being blocked.

**Acceptance Criteria:**
- Warning is shown inline on the skill detail view
- Warning lists each unmet prerequisite by name and required niveau
- The skill remains fully accessible (descriptors, resources) despite the warning
- No warning shown if all prerequisites are met or the skill has no prerequisites

**Dependencies:** SF-02, SF-07
**FRs:** FR8

---

## Epic 3: Goals & Readiness

### SF-10 — Coach assigns goal to consultant

**As a** coach,
**I want** to assign a skill goal to a consultant,
**so that** they have a clear, structured target to work toward.

**Acceptance Criteria:**
- Coach can create a goal for any consultant on their team: skill, current niveau, target niveau, deadline, optional linked resources
- Goal is immediately visible on the consultant's roadmap and goals list
- Coach can assign multiple goals per consultant
- Coach can edit or remove a goal
- Only `manager` role can write goals (server-side enforced)

**Dependencies:** SF-05
**FRs:** FR16

---

### SF-11 — Consultant views active goals

**As a** consultant,
**I want** to see my active goals in one place,
**so that** I know exactly what I'm expected to achieve and by when.

**Acceptance Criteria:**
- Goals list shows: skill name, current niveau, target niveau, deadline, linked resources
- Overdue goals are visually flagged
- Completed goals (validated by coach) are distinguishable from active goals
- Goals list is accessible from both the roadmap and a dedicated "Goals" view

**Dependencies:** SF-10
**FRs:** FR17

---

### SF-12 — Consultant raises readiness flag

**As a** consultant,
**I want** to signal to my coach that I believe I'm ready for a skill validation,
**so that** my coach knows to review and validate my progress in our next session.

**Acceptance Criteria:**
- Consultant can raise a readiness flag on any active goal
- Only one active flag allowed per goal at a time (button disabled if flag is already raised)
- Flag appears on the coach's dashboard immediately
- Consultant can retract a flag they raised (before the coach acts on it)

**Dependencies:** SF-11
**FRs:** FR18

---

### SF-13 — Readiness flag aging indicator

**As a** coach,
**I want** to see how long ago each readiness flag was raised,
**so that** I can prioritise consultants who have been waiting longest.

**Acceptance Criteria:**
- Flag age shown as days elapsed: "Raised 0 days ago", "Raised 3 days ago", etc.
- Age is computed from `raisedAt` timestamp, displayed in real time
- Flags older than 7 days receive a visual emphasis (e.g., amber colour)

**Dependencies:** SF-12
**FRs:** FR19, FR20

---

## Epic 4: Resource Library

### SF-14 — Browse resource library

**As any** authenticated user,
**I want** to browse the resource library,
**so that** I can discover learning materials relevant to my skills.

**Acceptance Criteria:**
- Resources list shows: title, type (article/video/book/course/other), linked skill, applicable niveau range, rating summary
- Resources can be filtered by skill and/or type
- Any authenticated user (learner, manager, backoffice) can access the library

**Dependencies:** SF-01
**FRs:** FR21

---

### SF-15 — Contribute a resource to the library

**As any** authenticated user,
**I want** to add a resource to the library,
**so that** the team's knowledge base grows organically.

**Acceptance Criteria:**
- Form fields: title (required), URL (required, valid URL), type (required), linked skill (required), fromLevel, toLevel, optional description
- Submitted resource is immediately visible in the library
- Submitter is recorded on the resource (`contributedBy`, `contributedAt`)

**Dependencies:** SF-14
**FRs:** FR22

---

### SF-16 — Mark resource as completed (evidence)

**As a** consultant,
**I want** to mark a resource as completed,
**so that** my progress is recorded and visible to my coach.

**Acceptance Criteria:**
- Consultant can mark any resource as completed; completion is recorded as evidence against a linked goal if one exists
- Completion is visible in the consultant's activity history
- A resource can only be marked completed once per consultant (marking is idempotent)
- Completion date is stored

**Dependencies:** SF-14, SF-11
**FRs:** FR23

---

### SF-17 — Rate a resource

**As any** authenticated user,
**I want** to rate a resource (thumbs up / thumbs down),
**so that** the community can surface the most useful learning materials.

**Acceptance Criteria:**
- Each user can rate a resource once; changing the rating replaces the previous one
- Resource shows aggregate rating: upvote count, downvote count
- Rating is visible in the library list

**Dependencies:** SF-14
**FRs:** FR24

---

## Epic 5: Coach Dashboard & Team Management

### SF-18 — Coach team overview dashboard

**As a** coach,
**I want** a single-screen overview of all my consultants,
**so that** I can spot who needs attention without opening individual profiles.

**Acceptance Criteria:**
- Lists all consultants assigned to the coach's team
- Each row shows: name, active goal count, readiness flag count, last activity date
- Dashboard is the default landing page for `manager` role
- Data reflects real state (no hardcoded values)

**Dependencies:** SF-10, SF-12
**FRs:** FR25, FR28, FR29

---

### SF-19 — Readiness flags surfaced on dashboard

**As a** coach,
**I want** to see all active readiness flags with their age on my dashboard,
**so that** I can act on them without manually checking each consultant.

**Acceptance Criteria:**
- Flags section shows: consultant name, skill, flag age
- Sorted by age descending (oldest first)
- Clicking a flag navigates to that consultant's profile
- Flag count badge visible in dashboard navigation

**Dependencies:** SF-13, SF-18
**FRs:** FR26

---

### SF-20 — Inactivity alerts on dashboard

**As a** coach,
**I want** to see which consultants have had no activity for 3+ weeks,
**so that** I can proactively re-engage them before they disengage.

**Acceptance Criteria:**
- Consultants with no recorded activity (resource completion, flag raised, goal progress) for ≥21 days are flagged
- Inactivity indicator shown on the consultant row in the dashboard
- "No activity" is defined as: no resource completed, no flag raised, no goal update in 21 days

**Dependencies:** SF-18, SF-16, SF-12
**FRs:** FR27

---

### SF-21 — Navigate to consultant profile from dashboard

**As a** coach,
**I want** to open any consultant's full profile from the dashboard,
**so that** I can review their history before a coaching session.

**Acceptance Criteria:**
- Each consultant row on the dashboard links to their profile
- Profile shows: assigned profile, active goals, skill validations received, resources completed, readiness flags raised (activity history)
- Coach can only access consultants on their own team (enforced server-side)

**Dependencies:** SF-18, SF-30 (activity history)
**FRs:** FR29, FR30

---

### SF-22 — Consultant activity history for coach

**As a** coach,
**I want** to see a chronological activity history for each consultant,
**so that** I walk into every session fully informed without digging through emails or memory.

**Acceptance Criteria:**
- Activity feed shows: resources completed (with date), readiness flags raised (with date), skill validations received (avec niveau change + coach note), goals set/updated
- Sorted most-recent-first
- Covers all activity since account creation

**Dependencies:** SF-16, SF-12, SF-24
**FRs:** FR30

---

## Epic 6: Live Session & Skill Validation

### SF-23 — Enter live session mode

**As a** coach,
**I want** to enter a focused live session view for a specific consultant,
**so that** distractions are removed and I can move quickly during the coaching session.

**Acceptance Criteria:**
- "Start Session" button available on any consultant's profile (coach only)
- Live session mode collapses the UI to show only: pending validations + active goals for that consultant
- Session is considered open from the moment "Start Session" is pressed
- Session can be exited; exiting records the session end time

**Dependencies:** SF-21, SF-11
**FRs:** FR31, FR32

---

### SF-24 — 2-tap skill validation

**As a** coach,
**I want** to validate a consultant's skill niveau with minimal taps,
**so that** I can process multiple validations in a single session without friction.

**Acceptance Criteria:**
- Tap 1: select skill from the pending validations list
- Tap 2: select the new validated niveau (current niveau pre-selected as default)
- Validation is saved on tap 2 — no extra confirmation step
- Validation record stores: `skillId`, `consultantId`, `validatedByCoachId`, `validatedAt`, `newNiveau` — immutable once written
- Only `manager` role can POST validations (server-side enforced)

**Dependencies:** SF-23
**FRs:** FR33, FR36

---

### SF-25 — Inline session notes

**As a** coach,
**I want** to add a short note during a live session,
**so that** the coaching conversation is captured for future reference.

**Acceptance Criteria:**
- Free-text note field available in live session mode
- Note is saved with the session record (`sessionId`, `coachId`, `consultantId`, `notes`, `sessionDate`)
- Notes are visible to the coach on the consultant's activity history
- Notes are NOT exposed to `backoffice` aggregate views (privacy constraint)

**Dependencies:** SF-23
**FRs:** FR34, FR37

---

### SF-26 — SMART goal setting in live session

**As a** coach,
**I want** to set a new goal for a consultant during or after a live session,
**so that** the next growth cycle starts before the session ends.

**Acceptance Criteria:**
- "Add Goal" available in live session mode (and on consultant profile)
- Same goal form as SF-10 (skill, current/target niveau, deadline, linked resources)
- New goal immediately appears on the consultant's roadmap
- Session does not need to be active to set a goal — coach can set goals outside of a session too

**Dependencies:** SF-23, SF-10
**FRs:** FR35

---

### SF-27 — Session recorded in activity history

**As a** coach,
**I want** completed sessions to be automatically recorded,
**so that** there's a permanent audit trail of all coaching interactions.

**Acceptance Criteria:**
- Each closed session creates an immutable session record: date, coach, consultant, notes, validations made
- Session appears in consultant activity history (SF-22)
- Validation records within the session are linked to `sessionId`

**Dependencies:** SF-25, SF-24, SF-22
**FRs:** FR37

---

## Epic 7: Seniority Tracking

### SF-28 — Seniority threshold rulesets per profile

**As a** platform admin,
**I want** to define seniority thresholds for each competence centre profile,
**so that** the system can compute each consultant's progress toward Junior/Medior/Senior.

**Acceptance Criteria:**
- Seniority ruleset is a list of `{skillId, minNiveau}` pairs per `{profileId, seniorityLevel}`
- Three seniority levels: Junior, Medior, Senior
- Rulesets are seeded as part of the initial data load (SF-03)
- Rulesets are readable via API; editable by `backoffice` only (post-MVP UI; seed data sufficient for MVP)

**Dependencies:** SF-04, SF-03
**FRs:** FR38

---

### SF-29 — Seniority progress indicator for consultant

**As a** consultant,
**I want** to see how many of the Medior/Senior threshold criteria I currently meet,
**so that** I have a clear, motivating view of progress toward my next seniority level.

**Acceptance Criteria:**
- Display: *"You meet X/Y [next level] requirements."*
- Counted at read time against current validated skill niveaux
- Shows progress toward the next seniority level above the consultant's current level
- Visible on the consultant's roadmap / profile page

**Dependencies:** SF-28, SF-24
**FRs:** FR39

---

## Epic 8: Admin User Management

### SF-30 — Create user account with role and team

**As an** admin,
**I want** to create new user accounts and assign their role and team,
**so that** new hires are onboarded to the platform in minutes.

**Acceptance Criteria:**
- Admin form: display name, email, role (backoffice/manager/learner), team assignment (multi-select)
- Created user can log in immediately with a temporary password or invite link
- New user is visible in the admin user list
- Only `backoffice` role can access user management (server-side enforced)

**Dependencies:** none (auth scaffold already in place)
**FRs:** FR5, FR40

---

### SF-31 — Archive user account (soft delete)

**As an** admin,
**I want** to archive a departing consultant's account,
**so that** they can no longer log in but all their coaching history is preserved.

**Acceptance Criteria:**
- Archived accounts: login disabled, invisible to active coach dashboards, removed from team lists
- All coaching records (sessions, validations, goals, resource completions) remain intact
- Archive action is logged with `archivedAt` and `archivedBy`
- Archived accounts do NOT appear in active user lists for any role

**Dependencies:** SF-30
**FRs:** FR41

---

### SF-32 — Restore archived account

**As an** admin,
**I want** to restore an archived user account,
**so that** a returning employee can resume where they left off.

**Acceptance Criteria:**
- Admin can view a list of archived accounts
- Restoring an account re-enables login and makes the consultant visible again
- All historical coaching data is immediately accessible after restore
- Restore action is logged

**Dependencies:** SF-31
**FRs:** FR42

---

### SF-33 — View consultants without an active coach

**As an** admin,
**I want** to see all consultants who are not currently assigned to an active coach,
**so that** I can reassign orphaned consultants quickly.

**Acceptance Criteria:**
- Admin view lists consultants where: assigned coach account is archived, or no coach has been assigned
- Shows consultant name, team, last coaching session date
- Admin can reassign a consultant to a new coach from this view

**Dependencies:** SF-31, SF-05
**FRs:** FR43

---

## Story Summary

| Epic | Stories | FRs covered |
|---|---|---|
| 1 — Skill Catalogue | SF-01 to SF-04 | FR6, FR7, FR8, FR9, FR10 |
| 2 — Consultant Roadmap | SF-05 to SF-09 | FR11–FR15, FR8 |
| 3 — Goals & Readiness | SF-10 to SF-13 | FR16–FR20 |
| 4 — Resource Library | SF-14 to SF-17 | FR21–FR24 |
| 5 — Coach Dashboard | SF-18 to SF-22 | FR25–FR30 |
| 6 — Live Session & Validation | SF-23 to SF-27 | FR31–FR37 |
| 7 — Seniority Tracking | SF-28 to SF-29 | FR38–FR39 |
| 8 — Admin User Management | SF-30 to SF-33 | FR5, FR40–FR43 |

**Total: 33 stories covering all 43 MVP functional requirements.**

---

## Suggested Implementation Order

Given the dependency graph and demo script (consultant loop → coach loop):

```
Phase A — Data model foundation
  SF-01 → SF-02 → SF-03 → SF-04 → SF-28

Phase B — Core consultant experience
  SF-05 → SF-06 → SF-07 → SF-09
  SF-10 → SF-11 → SF-12 → SF-13
  SF-08 (depends on goals)

Phase C — Resource library (parallel with B)
  SF-14 → SF-15 → SF-16 → SF-17

Phase D — Coach tools
  SF-18 → SF-19 → SF-20 → SF-21 → SF-22
  SF-23 → SF-24 → SF-25 → SF-26 → SF-27
  SF-29

Phase E — Admin (parallel with D)
  SF-30 → SF-31 → SF-32 → SF-33
```
