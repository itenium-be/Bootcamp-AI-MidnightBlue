# SkillForge — Implementation Order & Story Dependencies

**Generated:** 2026-03-13
**Source:** user-stories.md

---

## Dependency Graph

```
SF-01 ──► SF-02 ──► SF-03 ──► SF-04 ──► SF-05 ──► SF-06 ──► SF-07 ──► SF-09
                                │                    │
                                └──► SF-28           └──► SF-08 (needs SF-10 too)
                                      │
                                      └──► SF-29 (needs SF-24 too)

SF-05 ──► SF-10 ──► SF-11 ──► SF-12 ──► SF-13
                    │
                    └──► SF-16 ──► SF-20 (needs SF-18 too)

SF-01 ──► SF-14 ──► SF-15
                    ├──► SF-16
                    └──► SF-17

SF-10, SF-12 ──► SF-18 ──► SF-19 (needs SF-13 too)
                            ├──► SF-20 (needs SF-16 too)
                            ├──► SF-21 (needs SF-30 too)
                            └──► SF-22 (needs SF-16, SF-12, SF-24)

SF-21, SF-11 ──► SF-23 ──► SF-24 ──► SF-27 (needs SF-25, SF-22 too)
                            └──► SF-25

SF-23, SF-10 ──► SF-26

SF-30 ──► SF-31 ──► SF-32
                    └──► SF-33 (needs SF-05 too)
```

---

## Proposed Implementation Order

| Order | Story | Reason |
|---|---|---|
| 1 | **SF-30** | Admin/auth foundation — no deps, unblocks user onboarding |
| 2 | **SF-01** | Core data model — everything depends on it |
| 3 | **SF-02** | Prerequisite links — needed before seeding |
| 4 | **SF-03** | Seed catalogue — needed for profiles |
| 5 | **SF-04** | Profile entity — needed for consultant assignment |
| 6 | **SF-28** | Seniority rulesets — seeded together with SF-03/SF-04 |
| 7 | **SF-05** | Assign consultant to profile — unblocks roadmap & goals |
| 8 | **SF-10** | Coach assigns goals — needed by roadmap, dashboard, session |
| 9 | **SF-11** | Consultant views goals — needed by readiness, resources, session |
| 10 | **SF-06** | Consultant roadmap (progressive) |
| 11 | **SF-14** | Browse resource library |
| 12 | **SF-12** | Raise readiness flag |
| 13 | **SF-07** | Expand roadmap (needs SF-06 + SF-02) |
| 14 | **SF-09** | Skill dependency warning UI (needs SF-07) |
| 15 | **SF-13** | Flag aging indicator |
| 16 | **SF-15** | Contribute resource |
| 17 | **SF-16** | Mark resource completed (needs SF-14 + SF-11) |
| 18 | **SF-17** | Rate resource |
| 19 | **SF-08** | First-login experience (needs SF-06 + SF-10) |
| 20 | **SF-18** | Coach dashboard |
| 21 | **SF-19** | Readiness flags on dashboard (needs SF-13 + SF-18) |
| 22 | **SF-20** | Inactivity alerts (needs SF-18 + SF-16 + SF-12) |
| 23 | **SF-31** | Archive user account (needs SF-30) |
| 24 | **SF-21** | Navigate to consultant profile (needs SF-18 + SF-30) |
| 25 | **SF-23** | Enter live session mode (needs SF-21 + SF-11) |
| 26 | **SF-24** | 2-tap skill validation |
| 27 | **SF-25** | Inline session notes |
| 28 | **SF-26** | SMART goal in live session (needs SF-23 + SF-10) |
| 29 | **SF-22** | Consultant activity history (needs SF-16 + SF-12 + SF-24) |
| 30 | **SF-27** | Session recorded in history (needs SF-25 + SF-24 + SF-22) |
| 31 | **SF-29** | Seniority progress indicator (needs SF-28 + SF-24) |
| 32 | **SF-32** | Restore archived account (needs SF-31) |
| 33 | **SF-33** | Consultants without coach (needs SF-31 + SF-05) |

---

## Parallelization Opportunities

- **SF-14–SF-17** (resource library, stories 11–18) can run in parallel with **SF-06–SF-09** (roadmap, stories 10–14).
  - Exception: SF-16 (mark resource completed) depends on SF-11 (active goals) and must wait for that to be done.
- **SF-30–SF-33** (admin, stories 1 + 23–33) can run in parallel with most other epics at any point.
- **SF-28** (seniority rulesets) can be done alongside SF-03/SF-04 since it is seeded in the same data load.
