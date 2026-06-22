# Redesign Implementation Plan for KIDIO App (Completed)

This document outlines the step-by-step plan to redesign the KIDIO frontend screens to match the premium Figma mockups provided in the screenshots.

## 1. Shell & Navigation Redesign (`topics_list_screen.dart`) - [x] **Completed**
* **Goal**: Convert the bottom navigation from a 2-tab system to a 4-tab system matching Screenshot 8 ("Map", "Quests", "Awards", "Parent") with custom animations, active styling (pink color, active dot below).
* **Changes**:
  * Adjust `pages` list to contain:
    1. `_buildTopicsGrid()` (Map tab)
    2. `const QuestScreen(isTab: true)` (Quests tab)
    3. `const AchievementsScreen(isTab: true)` (Awards tab)
    4. `const ParentDashboardScreen(isTab: true)` (Parent tab)
  * Implement PIN validation when tapping on the **Parent** tab: if verification succeeds, transition to index 3.
  * Omit individual app bars/scaffolds from Quests, Awards, and Parent child screens when running as tabs. Keep the main header bar (Avatar, Stars, Streak, "KIDIO" title) active for Map, Quests, and Awards.

## 2. Quests Tab (`quest_screen.dart`) - [x] **Completed**
* **Goal**: Integrate `QuestScreen` smoothly as a bottom tab.
* **Changes**:
  * Add an `isTab` parameter to `QuestScreen`.
  * If `isTab` is true, omit the `Scaffold` and `AppBar`. Directly display the body wrapped in `SafeArea`.
  * Update `TopicsListScreen`'s daily quest banner click action to switch index to tab 1 instead of pushing a new screen.

## 3. Treasure Room / Awards Tab (`achievements_screen.dart`) - [x] **Completed**
* **Goal**: Recreate the "Treasure Room" screen from Screenshot 8.
* **Changes**:
  * Add `isTab` parameter. If true, omit top header summary (which is redundant with the main AppBar).
  * **Top section**: Create a deep purple gradient box with stars, trophy icon, gold coin sack icon, and the "120 Total Stars" yellow badge.
  * **Bottom section**: Render "My Badges" white rounded card container showing progress "4 / 8 ✔".
  * Create a 2x2 grid representing the 8 badges (4 active/colored and checked, 4 grayed out/locked with a lock icon):
    1. **First Lesson** (yellow background, book icon, checked, "+10 stars")
    2. **10 Lessons Done** (orange background, target icon, checked, "+50 stars")
    3. **100 Stars Earned** (yellow background, star icon, checked, "+100 stars")
    4. **7 Day Streak** (pink background, fire icon, checked, "+70 stars")
    5. **Pronunciation Master** (gray background, microphone icon, locked, "Keep playing to unlock!")
    6. **Quiz Champion** (gray background, trophy icon, locked, "Keep playing to unlock!")
    7. **Island Explorer** (gray background, map icon, locked, "Keep playing to unlock!")
    8. **Boss Slayer** (gray background, swords icon, locked, "Keep playing to unlock!")

## 4. Choose a Lesson Screen (`topic_detail_screen.dart`) - [x] **Completed**
* **Goal**: Redesign the lesson list to look like Screenshot 2.
* **Changes**:
  * Convert vertical ListTile list into a list of card items with:
    * Lesson difficulty badge (e.g. "Beginner" in light blue or "Elementary" in orange).
    * Lesson index title (e.g. "Lesson 1: Animals").
    * 5 star progress rating indicator.
    * Progress bar indicator.
    * 3D button matching the state: "Again!" (green), "Continue" (orange), "Start ->" (pink), or locked state (gray).

## 5. Lesson Adventure Hub (`lesson_detail_screen.dart`) - [x] **Completed**
* **Goal**: Redesign activity list to match Screenshot 3.
* **Changes**:
  * Clean up standard ListTiles. Replace with large gradient card buttons:
    1. **Learn Vocabulary**: Pink/Red gradient card with panda illustration or speech bubbles.
    2. **Listening Games**: Blue/Cyan gradient card.
    3. **Challenge**: Purple gradient card.
    4. **Quiz Challenge**: Gold/Orange gradient card.
    5. **Final Boss**: Gray locked card with scattered stars and padlock icon.

## 6. Learn Vocabulary Screen (`vocab_learn_screen.dart`) - [x] **Completed**
* **Goal**: Recreate the vocabulary detail cards from Screenshot 4.
* **Changes**:
  * White card layout with rounded corners and drop shadow.
  * Top half: Orange banner card containing the animal face illustration (emoji) and a speaker icon.
  * Bottom half: Target word in large orange font ("DOG"), phonetics text, and a prominent orange "Listen!" button.
  * Bottom mascot section: Display the winking panda mascot with speech bubble ("Practice saying: 'DOG'").

## 7. Listening Game Screen (`listening_game_screen.dart`) - [x] **Completed**
* **Goal**: Recreate the 2x2 quiz layout in Screenshot 5.
* **Changes**:
  * Header showing round progress ("Round 1 of 5" with dots indicator).
  * Center Listening Speaker: Dark blue card with music note details, containing a large speaker icon in a circular border.
  * Options: 2x2 grid of option cards displaying the animal face/emoji **without** text labels.
  * Bottom mascot: Panda saying "Tap the speaker first! 🔊".

## 8. Pronunciation Challenge Screen (`pronunciation_challenge_screen.dart`) - [x] **Completed**
* **Goal**: Redesign to match the microphone-based challenge in Screenshot 6.
* **Changes**:
  * Header state pills: "Idle" (blue active), "Recording" (gray), "Result" (gray).
  * Main target word card: Orange card top with dog face image, white card bottom with "DOG" text, pronunciation text, and an orange "Listen!" button.
  * Recording controller: Centered large blue microphone button.
  * Bottom mascot: Panda saying "Ready? You can do it! 🐾 Tap the big mic and say: 'DOG' 🐶".

## 9. Quiz Challenge Screen (`vocabulary_quiz_screen.dart`) - [x] **Completed**
* **Goal**: Redesign the multiple-choice quiz in Screenshot 7.
* **Changes**:
  * Main Question card: Purple gradient card with questions (e.g. "Which animal says 'Meow'?", "Listen to the sound clue!").
  * Options: 2x2 grid of options (Dog, Cat, Lion, Rabbit) showing animal face images **with** text labels below them.
  * Bottom mascot: Panda saying "Tap the right answer! 👈".
