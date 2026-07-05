using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the Engineering Practices track. Content covers Git and version control,
/// continuous integration and delivery (CI/CD), and clean-code practices (naming, small functions,
/// DRY/KISS/YAGNI, code smells, code review, and testing strategy). The SOLID principles are
/// intentionally omitted here — they are covered in the Design Patterns bank — so this track focuses
/// on workflow and craftsmanship rather than object-oriented design. The per-question factory methods
/// are partitioned into subcategory lists (GitAndVersionControl, ContinuousIntegrationDelivery,
/// CleanCode).
/// </summary>
/// <remarks>
/// All questions are single-correct to satisfy the <c>MultipleChoice</c> Domain invariant
/// (exactly one correct option per question).
/// </remarks>
public static class EngineeringPracticesQuestions
{
    public static IReadOnlyList<Question> GitAndVersionControl(Guid categoryId) =>
    [
        Q01_GitCommit(categoryId),
        Q02_StagingArea(categoryId),
        Q03_MergeVsRebase(categoryId),
        Q04_GitPull(categoryId),
        Q05_WhatIsBranch(categoryId),
        Q06_CherryPick(categoryId),
        Q07_GitIgnore(categoryId),
        Q08_FastForward(categoryId),
        Q21_GitStash(categoryId),
        Q22_ResetVsRevert(categoryId),
        Q23_ConventionalCommits(categoryId),
        Q25_PullRequestPurpose(categoryId),
    ];

    public static IReadOnlyList<Question> ContinuousIntegrationDelivery(Guid categoryId) =>
    [
        Q09_ContinuousIntegration(categoryId),
        Q10_DeliveryVsDeployment(categoryId),
        Q11_CiPipelinePurpose(categoryId),
        Q12_BuildOnceDeployMany(categoryId),
        Q13_FeatureBranchFlow(categoryId),
        Q14_BlueGreenDeployment(categoryId),
        Q24_SemanticVersioning(categoryId),
        Q26_TrunkBasedDevelopment(categoryId),
    ];

    public static IReadOnlyList<Question> CleanCode(Guid categoryId) =>
    [
        Q15_MeaningfulNames(categoryId),
        Q16_Dry(categoryId),
        Q17_Yagni(categoryId),
        Q18_CodeSmell(categoryId),
        Q19_CodeReview(categoryId),
        Q20_TestPyramid(categoryId),
        Q27_TechnicalDebt(categoryId),
        Q28_Refactoring(categoryId),
        Q29_BoyScoutRule(categoryId),
        Q30_StaticAnalysis(categoryId),
    ];

    private static Question Q01_GitCommit(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does `git commit` do?",
            explanation:
                "`git commit` records a snapshot of the staged changes into the local repository's history, " +
                "creating a new commit object. It does not contact any remote — sharing happens later with " +
                "`git push`. Only changes that were staged with `git add` are included.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Records the staged changes as a new snapshot in the local repository history", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Uploads your changes to the remote repository for everyone to see",           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Downloads the latest changes from the remote into your working tree",         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Permanently deletes the staging area and all uncommitted work",               isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_StagingArea(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the purpose of the staging area (index) in Git?",
            explanation:
                "The staging area lets you assemble exactly which changes go into the next commit. `git add` " +
                "moves changes from the working directory into the staging area; `git commit` then snapshots " +
                "only what is staged. This lets you split unrelated edits into separate, focused commits.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It holds the changes selected to be included in the next commit", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It is a backup copy of the remote repository",                    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It stores commits that have not yet been pushed",                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is where merge conflicts are permanently archived",            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_MergeVsRebase(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the key difference between `git merge` and `git rebase`?",
            explanation:
                "`merge` combines two branches by creating a merge commit, preserving the actual branching " +
                "history. `rebase` replays your commits on top of another branch, rewriting them into a new " +
                "linear sequence. Rebase produces cleaner history but rewrites commit hashes, so it should be " +
                "avoided on shared/public branches.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`merge` preserves history with a merge commit; `rebase` rewrites commits onto a new base for a linear history", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`merge` deletes the source branch; `rebase` keeps it",                                                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are identical; `rebase` is a deprecated alias for `merge`",                                                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`merge` works only locally; `rebase` works only with remotes",                                                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_GitPull(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "`git pull` is essentially a combination of which two commands?",
            explanation:
                "`git pull` runs `git fetch` (download new commits from the remote into remote-tracking " +
                "branches) followed by `git merge` (integrate them into your current branch). With " +
                "`--rebase` the second step becomes a rebase instead of a merge.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`git fetch` followed by `git merge`",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`git add` followed by `git commit`",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`git commit` followed by `git push`",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`git clone` followed by `git fetch`",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_WhatIsBranch(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is a branch in Git, at its core?",
            explanation:
                "A branch is simply a lightweight, movable pointer to a commit. Creating a branch just writes " +
                "a new pointer — it does not copy files — which is why branching in Git is cheap and fast. As " +
                "you commit, the branch pointer advances to the newest commit.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A movable pointer to a commit",                            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A full physical copy of the entire repository",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A snapshot of the working directory saved to disk",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A separate remote server that mirrors your commits",       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_CherryPick(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What does `git cherry-pick <commit>` do?",
            explanation:
                "`git cherry-pick` applies the changes from a single specific commit onto your current " +
                "branch, creating a new commit with the same diff (but a new hash). It is useful for pulling " +
                "one bug fix from another branch without merging that entire branch.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Applies the changes from one specific commit onto the current branch as a new commit", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Merges an entire branch but skips any merge conflicts",                                isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Reverts the named commit and removes it from history",                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Selects the best commit on a branch and discards the rest",                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_GitIgnore(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the purpose of a `.gitignore` file?",
            explanation:
                "`.gitignore` lists file patterns that Git should not track — build output (`bin/`, `obj/`, " +
                "`node_modules/`), local config, secrets, and IDE files. It keeps generated and sensitive " +
                "files out of version control. Note: it only affects untracked files; already-tracked files " +
                "must be untracked explicitly.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It tells Git which untracked files and patterns to leave out of version control", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It lists the files that must always be committed on every push",                  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It encrypts sensitive files before they are committed",                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It defines which branches are allowed to be merged",                              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_FastForward(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "When does a Git merge result in a fast-forward (no merge commit)?",
            explanation:
                "A fast-forward happens when the target branch has not diverged — there are no commits on it " +
                "that the source branch lacks. Git can simply move the branch pointer forward to the source " +
                "tip, with no merge commit needed. If both branches have new commits, Git must create a merge " +
                "commit instead.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "When the target branch has no new commits of its own, so the pointer just advances", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Whenever there are no merge conflicts between the branches",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Only when merging into the `master` branch specifically",                             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "When the source branch is deleted immediately after the merge",                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_ContinuousIntegration(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is Continuous Integration (CI)?",
            explanation:
                "CI is the practice of frequently merging developers' changes into a shared branch, where each " +
                "integration is automatically built and tested. Catching integration problems early — on every " +
                "push — avoids the painful 'big bang' merges that come from long-lived divergent branches.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Frequently merging changes into a shared branch, with each merge automatically built and tested", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Deploying the application to production several times per hour",                                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Writing all code in a single long-lived branch to avoid merges",                                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Continuously monitoring servers for downtime and errors",                                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_DeliveryVsDeployment(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the difference between Continuous Delivery and Continuous Deployment?",
            explanation:
                "Both keep the application in an always-releasable state through automation. Continuous " +
                "Delivery stops short of production — a human approves the final release. Continuous Deployment " +
                "goes one step further: every change that passes the pipeline is released to production " +
                "automatically, with no manual gate.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Delivery keeps releases ready but a human approves the production push; Deployment releases automatically", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Delivery releases to production automatically; Deployment requires manual approval",                         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are two names for the exact same fully automated process",                                              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Delivery only runs tests; Deployment only builds the artifact",                                              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_CiPipelinePurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Why is it valuable to run the build and automated tests in a CI pipeline rather than relying only on each developer's machine?",
            explanation:
                "A CI pipeline builds and tests in a clean, consistent, shared environment, so it catches " +
                "'works on my machine' problems — missing dependencies, uncommitted files, environment drift — " +
                "before code is merged. It provides an objective, repeatable gate that every change must pass.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It validates every change in a clean, consistent environment, catching 'works on my machine' issues", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It removes the need for developers to ever run tests locally",                                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It makes the application run faster in production",                                                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It automatically fixes failing tests before merging",                                                  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_BuildOnceDeployMany(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Why is 'build once, deploy many' considered a CI/CD best practice?",
            explanation:
                "Building a single immutable artifact once and promoting that same artifact through dev → " +
                "staging → production guarantees you test and ship the exact same bits. Rebuilding per " +
                "environment risks subtle differences (dependency versions, timestamps, config) so that what " +
                "you tested isn't what you shipped. Environment-specific values are injected at deploy time, " +
                "not baked into separate builds.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Promoting one immutable artifact through every environment ensures you ship exactly what was tested", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Rebuilding for each environment is required to inject the correct source code",                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It lets each environment run a different version of the application on purpose",                       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It eliminates the need for automated tests in the pipeline",                                           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_FeatureBranchFlow(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In a feature-branch workflow with a protected main branch, how does work typically reach main?",
            explanation:
                "Work is done on a short-lived feature branch, then proposed via a pull request. The PR runs " +
                "CI and gets reviewed; only after checks pass and it is approved is it merged into the " +
                "protected main branch. Direct commits to main are blocked, which keeps main releasable and " +
                "every change reviewed.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Via a pull request from a feature branch that must pass CI and review before being merged", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "By committing directly to main and fixing problems afterwards",                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "By force-pushing the feature branch over main",                                            isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "By emailing a patch file to the repository administrator",                                 isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_BlueGreenDeployment(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What is a blue-green deployment?",
            explanation:
                "Blue-green deployment runs two identical production environments. One (blue) serves live " +
                "traffic while the new version is deployed and verified on the idle one (green); traffic is " +
                "then switched over. This gives near-zero downtime and an instant rollback — just switch " +
                "traffic back to the previous environment if something is wrong.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Running two identical environments and switching traffic to the new one, enabling zero-downtime releases and instant rollback", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Deploying to production only on alternating days to reduce load",                                                                isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Splitting the codebase into two repositories, one per team",                                                                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Releasing a feature to a small percentage of users before everyone else",                                                        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_MeaningfulNames(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which variable name best reflects clean-code naming guidance?",
            explanation:
                "Clean code favours intention-revealing names: a reader should understand a variable's purpose " +
                "without comments or guesswork. `elapsedTimeInDays` states both what it holds and its unit. " +
                "Single letters like `d`, vague names like `data`, or cryptic abbreviations force the reader to " +
                "reverse-engineer the meaning.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`elapsedTimeInDays`", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`d`",                 isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`data`",             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`tmp2`",             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_Dry(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the DRY principle stand for, and what does it advise?",
            explanation:
                "DRY — 'Don't Repeat Yourself' — advises that each piece of knowledge or logic should have a " +
                "single, authoritative representation. Duplicated logic means a change must be made in many " +
                "places and is easy to get wrong. Note: DRY is about knowledge duplication, not blindly " +
                "deduplicating any code that happens to look similar.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "\"Don't Repeat Yourself\" — each piece of logic should have a single authoritative source", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "\"Do Repeat Yourself\" — duplicate code for safety and redundancy",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "\"Develop, Review, Yield\" — a three-stage release process",                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "\"Don't Rush Yourself\" — a guideline about coding pace",                                     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_Yagni(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the YAGNI principle advise?",
            explanation:
                "YAGNI — 'You Aren't Gonna Need It' — advises against building functionality on the speculation " +
                "that it will be needed later. Premature generality adds complexity and maintenance cost for " +
                "features that often never materialise. Build what the current requirement needs; add more when " +
                "a real need appears.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Don't build functionality until it is actually required",            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Always add extra configuration options for future flexibility",      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Generalise every class so it can be reused everywhere",              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Write the documentation before any of the code",                     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_CodeSmell(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What is a 'code smell'?",
            explanation:
                "A code smell is a surface-level symptom in the code that often signals a deeper design " +
                "problem — for example a very long method, a huge class, long parameter lists, or duplicated " +
                "logic. A smell is not a bug; the code may work correctly. It is a heuristic hint that " +
                "refactoring may be warranted.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A surface symptom (e.g. a long method or duplicated logic) that hints at a deeper design problem", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A compiler error that prevents the code from building",                                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A runtime exception thrown when invalid input is supplied",                                         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A security vulnerability detected by a static analysis tool",                                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_CodeReview(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is a primary benefit of code review?",
            explanation:
                "Code review catches defects and design issues early, before code is merged, and spreads " +
                "knowledge of the codebase across the team. It also nudges authors toward clearer, more " +
                "maintainable code. It is a quality and collaboration practice — not a way to assign blame.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Catching issues early and sharing knowledge across the team before code is merged", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Replacing the need for any automated testing",                                      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Identifying which developer to blame for bugs",                                      isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Making the application run faster in production",                                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_TestPyramid(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What does the 'test pyramid' recommend about a project's test suite?",
            explanation:
                "The test pyramid recommends many fast, isolated unit tests at the base, fewer integration " +
                "tests in the middle, and only a small number of slow, brittle end-to-end (UI) tests at the " +
                "top. This keeps the suite fast and reliable. The 'ice-cream cone' anti-pattern inverts this — " +
                "mostly slow E2E tests — and leads to brittle, sluggish feedback.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Many fast unit tests at the base, fewer integration tests, and few slow end-to-end tests at the top", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Mostly end-to-end UI tests, with very few unit tests",                                                 isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "An equal number of unit, integration, and end-to-end tests",                                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Only integration tests, since they exercise the most code",                                             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q21_GitStash(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does `git stash` do?",
            explanation: "`git stash` saves uncommitted changes (staged and unstaged) onto a stack and reverts the working directory to a clean state, so you can switch contexts and reapply the changes later.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Temporarily shelves uncommitted changes and cleans the working directory", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Permanently deletes uncommitted changes", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Creates a new commit on the current branch", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Pushes local changes to the remote", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q22_ResetVsRevert(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "How does `git revert` differ from `git reset`?",
            explanation: "`git revert` creates a new commit that undoes a previous one, preserving history — safe for shared branches. `git reset` moves the branch pointer and can rewrite history, which is dangerous once pushed.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "revert adds a new commit that undoes changes; reset moves the branch pointer", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They are aliases for the same operation", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "revert deletes commits permanently; reset keeps history", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "revert only works on remote branches", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q23_ConventionalCommits(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the purpose of the Conventional Commits specification?",
            explanation: "Conventional Commits define a structured commit message format (e.g. `feat:`, `fix:`) that machines can parse to automate versioning and changelog generation, and that humans can read consistently.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A standard commit message format that enables automation like semantic versioning", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A rule that every commit must contain tests", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A limit on how many files a commit may touch", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A Git hook that signs commits with GPG", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q24_SemanticVersioning(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In semantic versioning (MAJOR.MINOR.PATCH), what triggers a MAJOR version bump?",
            explanation: "Under SemVer, MAJOR increments for incompatible (breaking) API changes, MINOR for backward-compatible new features, and PATCH for backward-compatible bug fixes.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A backward-incompatible (breaking) change", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Any new backward-compatible feature", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A bug fix that keeps the API stable", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A documentation-only update", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q25_PullRequestPurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the primary purpose of a pull request?",
            explanation: "A pull request proposes merging changes from one branch into another and provides a place for code review, automated checks, and discussion before the code is integrated.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "To propose changes for review and discussion before merging", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "To download the latest changes from the remote", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "To create a backup of the repository", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "To deploy the application to production", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q26_TrunkBasedDevelopment(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What characterizes trunk-based development?",
            explanation: "In trunk-based development, developers integrate small changes frequently into a single shared branch (the trunk), keeping branches short-lived to minimize merge conflicts and enable continuous integration.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Frequent small merges into one shared branch with short-lived branches", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Long-lived feature branches merged only at release time", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "One permanent branch per developer", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "No branching; everyone edits files directly on the server", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q27_TechnicalDebt(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the term 'technical debt' describe?",
            explanation: "Technical debt is the implied future cost of choosing an easy or quick solution now over a better approach that would take longer — like financial debt, it accrues 'interest' as it slows future work.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The future cost of taking shortcuts instead of better solutions", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Money owed to software vendors for licenses", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The number of open bugs in a tracker", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The time a build takes to complete", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q28_Refactoring(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is refactoring?",
            explanation: "Refactoring is improving the internal structure of code without changing its external behavior, making it easier to understand and cheaper to modify while keeping all tests green.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Improving code's internal structure without changing its behavior", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Adding new features to existing code", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Fixing a bug that changes program output", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Rewriting the application in a new language", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q29_BoyScoutRule(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the 'Boy Scout Rule' advise in software craftsmanship?",
            explanation: "The Boy Scout Rule, popularized by Robert C. Martin, says to always leave the code a little cleaner than you found it — small continuous improvements prevent gradual decay.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Always leave the code cleaner than you found it", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Never modify code you did not write", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Always write code in pairs", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Commit only at the end of the day", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q30_StaticAnalysis(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is static code analysis?",
            explanation: "Static analysis inspects source code without executing it, automatically detecting potential bugs, style violations, security issues, and code smells early in the development cycle.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Examining source code for issues without running it", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Measuring performance while the program runs", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Running the full test suite on every commit", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Monitoring memory usage in production", isCorrect: false, orderIndex: 3),
            ]);
    }
}
