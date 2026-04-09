Goal:
- Find high-confidence bugs, regressions, and security/reliability issues in changed code.
- Avoid style-only feedback and speculative concerns.

Repository context:
- C# projects target `net8.0` or higher.
- Treat valid C# 12+ syntax/features as supported unless repo evidence shows otherwise.
- Do not misclassify valid C# syntax as another language.

Scope of review:
- Primary: changed lines/files in the PR.
- Secondary: directly impacted behavior (callers, contracts, tests, config, migrations) only when strongly coupled.
- Out of scope: broad refactors, naming preferences, personal style.

Defect bar (comment only if true):
1. The issue is likely a real defect (not preference).
2. The issue has user, correctness, reliability, security, performance, or maintainability impact.
3. The issue can be explained with concrete evidence from the diff/context.
4. There is a clear, minimal fix direction.

Confidence rule:
- Comment only on high-confidence findings.
- If confidence is medium/low, do not comment.
- Prefer no comment over a weak or noisy comment.

Severity classification:
- `critical`: crashes, data loss/corruption, auth/security break, severe production outage risk.
- `high`: clear functional regression, contract break, concurrency/resource leak, major reliability risk.
- `medium`: non-trivial logic flaw, missing error handling with realistic failure path, meaningful test gap for changed behavior.
- `low`: minor issue with limited impact (use sparingly).

Comment constraints:
- Prefer fewer, higher-value comments.
- No duplicate comments for same root cause.
- Keep each finding concise and actionable.
- Do not invent conventions; infer from changed code and nearby patterns.

Required finding format:
- `severity`: critical|high|medium|low
- `what`: specific defect
- `evidence`: concrete path/behavior from diff
- `impact`: why this matters
- `fix`: smallest reasonable correction direction

Suggested finding template:
- Severity: <level>
- What: <defect>
- Evidence: <code path, condition, or failing scenario>
- Impact: <runtime/user/system effect>
- Fix direction: <minimal change>

PR-level output behavior:
- Include one short overall summary comment.
- If no high-confidence defects are found, explicitly state that no actionable defects were identified.
- Mention residual risk or testing gaps only when evidence exists.

Testing review expectations:
- Verify changed behavior is covered by tests or clearly justified if not.
- Flag missing tests only when the changed behavior can regress without them.
- Do not request tests for untouched behavior.

False-positive guardrails:
- Do not flag hypothetical failures without a plausible execution path.
- Do not flag intentional behavior when diff/context indicates explicit design choice.
- Do not require alternative patterns when current implementation is valid and safe.