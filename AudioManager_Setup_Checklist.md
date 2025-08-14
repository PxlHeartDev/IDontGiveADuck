# AudioManager Music System Setup Checklist

## Overview
This checklist ensures that the AudioManager music system is properly configured and working correctly in your Unity project.

## ✅ Pre-Setup Verification

### 1. Audio Files Verification
- [ ] Verify all audio files are present in `Assets/Audio/`:
  - [ ] `Menu.wav` - Menu background music
  - [ ] `tutorial.wav` - Tutorial level music
  - [ ] `action.wav` - Action level music  
  - [ ] `challenge.wav` - Challenge level music
  - [ ] `Boss.wav` - Boss level music
  - [ ] `DecoyDuck.mp3` - Decoy duck click sound
  - [ ] `GoodDuck.mp3` - Good duck click sound

### 2. Script Verification
- [ ] `AudioManager.cs` is present in `Assets/Scripts/Core/`
- [ ] `AudioManagerTester.cs` is present in `Assets/Scripts/Core/`
- [ ] `GameManager.cs` is present in `Assets/Scripts/Core/`

## ✅ Scene Setup

### 3. AudioManager GameObject
- [ ] Create an empty GameObject named "AudioManager"
- [ ] Attach the `AudioManager.cs` script to it
- [ ] Ensure it's marked as `DontDestroyOnLoad` (handled by script)

### 4. AudioManager Inspector Configuration
- [ ] **Audio Sources** section:
  - [ ] Music Source: Leave empty (auto-created by script)
  - [ ] SFX Source: Leave empty (auto-created by script)

- [ ] **Background Music** section:
  - [ ] Menu Music: Assign `Menu.wav`
  - [ ] Game Over Music: Assign `Boss.wav` (or create separate)
  - [ ] Victory Music: Assign `challenge.wav` (or create separate)

- [ ] **Level-Specific Music** section:
  - [ ] Tutorial Theme: Assign `tutorial.wav`
  - [ ] Action Theme: Assign `action.wav`
  - [ ] Challenge Theme: Assign `challenge.wav`
  - [ ] Boss Theme: Assign `Boss.wav`

- [ ] **UI Sounds** section:
  - [ ] Level Start Sound: Assign appropriate sound or leave empty
  - [ ] Level Complete Sound: Assign appropriate sound or leave empty
  - [ ] Game Over Sound: Assign appropriate sound or leave empty

- [ ] **Duck Sounds** section:
  - [ ] Duck Click Decoy Sound: Assign `DecoyDuck.mp3`
  - [ ] Duck Click Good Sound: Assign `GoodDuck.mp3`

- [ ] **Volume Settings** section:
  - [ ] Master Volume: Set to 1.0 (100%)
  - [ ] Music Volume: Set to 0.5 (50%) - increased from 0.2
  - [ ] SFX Volume: Set to 1.0 (100%)

## ✅ Testing Setup

### 5. AudioManagerTester Setup
- [ ] Create an empty GameObject named "AudioManagerTester"
- [ ] Attach the `AudioManagerTester.cs` script to it
- [ ] Enable "Enable Debug Logging" in the inspector

### 6. GameManager Setup
- [ ] Ensure GameManager GameObject exists in scene
- [ ] Verify GameManager has `GameManager.cs` script attached
- [ ] Check that GameManager is properly configured

## ✅ Runtime Testing

### 7. Console Verification
When you start the game, check the console for these messages:
- [ ] "AudioManager initialized"
- [ ] "AudioManager: Now playing music 'Menu' (Volume: 0.50)"
- [ ] No error messages about missing audio clips

### 8. Manual Testing with AudioManagerTester
Use the on-screen buttons or keyboard shortcuts:
- [ ] Press `1` - Should play menu music
- [ ] Press `2` - Should play tutorial music
- [ ] Press `3` - Should play action music
- [ ] Press `4` - Should play challenge music
- [ ] Press `5` - Should play boss music
- [ ] Press `0` - Should stop music
- [ ] Press `Space` - Should log audio status

### 9. Game Flow Testing
- [ ] Start game from menu - should transition to level music
- [ ] Complete a level - should play victory music
- [ ] Fail a level - should play game over music
- [ ] Return to menu - should play menu music

## ✅ Level Music Testing

### 10. Level-Specific Music Verification
Check that each level plays the correct music:
- [ ] Level 1-4: Should play `tutorial_theme`
- [ ] Level 5+: Should play `action_theme` (or as specified in level JSON)
- [ ] Boss levels: Should play `boss_theme`

### 11. Level JSON Verification
Verify level files have correct `backgroundMusic` field:
- [ ] `level_001.json`: `"backgroundMusic": "tutorial_theme"`
- [ ] `level_005.json`: `"backgroundMusic": "action_theme"`
- [ ] Check other levels as needed

## ✅ Troubleshooting

### 12. Common Issues and Solutions

**Issue: No music playing**
- [ ] Check AudioManager GameObject exists in scene
- [ ] Verify audio clips are assigned in inspector
- [ ] Check console for error messages
- [ ] Verify volume settings are not 0

**Issue: Music too quiet/loud**
- [ ] Adjust Music Volume in AudioManager inspector
- [ ] Check Master Volume setting
- [ ] Verify audio file quality

**Issue: Wrong music for level**
- [ ] Check level JSON `backgroundMusic` field
- [ ] Verify AudioManager has correct clips assigned
- [ ] Check console for music transition logs

**Issue: Music not transitioning**
- [ ] Verify GameManager events are firing
- [ ] Check AudioManager event subscriptions
- [ ] Look for console error messages

## ✅ Performance Verification

### 13. Audio Performance
- [ ] No audio stuttering during gameplay
- [ ] Smooth music transitions between levels
- [ ] No memory leaks from audio sources
- [ ] Audio doesn't interfere with game performance

## ✅ Final Verification

### 14. Complete Game Flow Test
- [ ] Start game → Menu music plays
- [ ] Start level → Level music plays
- [ ] Click ducks → Duck sounds play
- [ ] Complete level → Victory music plays
- [ ] Return to menu → Menu music plays
- [ ] All transitions are smooth and appropriate

## Notes

- The AudioManager uses a singleton pattern and persists across scenes
- Music volume was increased from 0.2 to 0.5 for better audibility
- Duck sounds are amplified 10x for better prominence
- All audio transitions are logged to console for debugging
- The AudioManagerTester provides manual testing capabilities

## Support

If you encounter issues:
1. Check the Unity Console for error messages
2. Use the AudioManagerTester to isolate problems
3. Verify all audio files are properly imported
4. Ensure AudioManager GameObject is in the scene
5. Check that all inspector fields are properly assigned
