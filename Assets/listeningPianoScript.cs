using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class listeningPianoScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMSelectable[] PianoKeys;
    public KMSelectable PlayButton;
    public KMSelectable Switch;

    public Transform[] Spinners;
    public GameObject Labels;
    public SwitchMovement switchMovement;

    private static readonly string[][] _noteNames = "C,C♯/Cs/D♭/Df,D,D♯/Ds/E♭/Ef,E,F,F♯/Fs/G♭/Gf,G,G♯/Gs/A♭/Af,A,A♯/As/B♭/Bf,B".Split(',').Select(s => s.Split('/')).ToArray();

    struct Note
    {
        public int Pitch;
        public int Length;
    }

    struct Song
    {
        public string Name { get; set; }
        public int Speed { get; set; }
        public string Melody { get; set; }

        private Note[] _notes;
        public Note[] Notes
        {
            get
            {
                if (_notes == null)
                {
                    var name = Name;
                    _notes = Melody.Split(' ').Select(s =>
                    {
                        int noteLength;
                        for (var pitch = 0; pitch < _noteNames.Length; pitch++)
                            foreach (var noteName in _noteNames[pitch])
                                if (s.StartsWith(noteName, StringComparison.InvariantCultureIgnoreCase) && int.TryParse(s.Substring(noteName.Length), out noteLength))
                                    return new Note { Pitch = pitch, Length = noteLength };
                        throw new InvalidOperationException($"The melody for {name} contains invalid note {s}.");
                    }).ToArray();
                }
                return _notes;
            }
        }
    }

    private static readonly Song[] Songs = new[]
    {
        new Song { Name = "Frosty the Snowman", Speed = 400, Melody = "ef4 c3 df1 ef2 af4 g1 af1 bf2 af2 g2 f2 ef1" },
        new Song { Name = "Tetris", Speed = 300, Melody = "g2 d1 ef1 f2 ef1 d1 c2 c1 ef1 g1" },
        new Song { Name = "Still Alive (Radio Mix)", Speed = 480, Melody = "bf1 a2 g3 a10 bf1 a2 g3 a2 f2 g1 c1" },
        new Song { Name = "Can Can", Speed = 300, Melody = "c4 d1 f1 e1 d1 g2 g2 g1 a1 e1 f1 d2 d1" },
        new Song { Name = "Wait (KTANE OST)", Speed = 260, Melody = "c2 g1 c1 c1 ef1 e1 f2 a2 ef1 f1 ef1 f1 ef1" },
        new Song { Name = "Fur Elise", Speed = 240, Melody = "g1 gf1 g1 gf1 g1 d1 f1 ef1 c1" },
        new Song { Name = "Spring (Vivaldi)", Speed = 480, Melody = "e2 e2 e2 d1 c1 g6 g1 f1 e2 e2 e2 d1 c1 g1" },
        new Song { Name = "Never Gonna Give You Up", Speed = 480, Melody = "c1 d1 f1 d1 a3 a3 g6 c1 d1 e1 c1 g3 g3 f1" },
        new Song { Name = "A Cruel Angel’s Thesis (Evangelion)", Speed = 520, Melody = "c4 ef4 f3 ef3 f2 f2 f2 bf2 af2 g1 f2 g1" },
        new Song { Name = "It’s Been So Long (The Living Tombstone)", Speed = 510, Melody = "d2 f1 a2 d1 f2 a1 d2 df1 c2 e1 bf2 c1 e2 bf1 g2 e1" },
        new Song { Name = "Happy Birthday", Speed = 390, Melody = "c2 c1 d3 c3 f3 e6 c2 c1 d3 c3 g3 f1" },
        new Song { Name = "Billie Jean", Speed = 240, Melody = "g1 d1 f1 g1 f1 d1 c1 d1 g1 d1 f1 g1 f1 d1 c1 d1" },
        new Song { Name = "Despacito", Speed = 1080, Melody = "f15 c3 f3 g3 af4 bf4 af4 g4 f4 ef4 df9 af9 af1" },
        new Song { Name = "Blue (Da Ba Dee)", Speed = 240, Melody = "e2 f2 d1 f1 g1 c1 e1 f2 d1 f1 a1 bf2 a1 g1 f1" },
        new Song { Name = "Among Us (Trap Remix)", Speed = 360, Melody = "d4 d2 f2 g2 af2 g2 f2 d6 c1 e1 d1" },
        new Song { Name = "Hello (OMFG)", Speed = 600, Melody = "c6 c2 d1 ef2 g1 af3 af3 af2 g1 f2 g1 ef1" },
        new Song { Name = "Axel F", Speed = 440, Melody = "d4 f3 d2 d1 g2 d2 c2 d4 a3 d2 d1 bf2 a2 f1" },
        new Song { Name = "Unity (TheFatRat)", Speed = 540, Melody = "a1 g2 f1 d2 c1 d5 a1 g2 f1 d2 c1 d5 a1 g2 f1 d2 c1 d3 f3 g3 a3 g1" },
        new Song { Name = "In the Hall of the Mountain King", Speed = 240, Melody = "c1 d1 ef1 f1 g1 ef1 g2 gf1 d1 gf2 f1 df1 f1" },
        new Song { Name = "Buddy Holly", Speed = 240, Melody = "f1 df1 f1 g1 a1 g1 f1 df1 c1" },
        new Song { Name = "Ode to Joy", Speed = 280, Melody = "e2 e2 f2 g2 g2 f2 e2 d2 c2 c2 d2 e2 e3 d1 d1" },
        new Song { Name = "Victory Jingle (KTANE)", Speed = 460, Melody = "d1 c1 d1 f3 g1 f1 g1 a3 c2 df2 d1" },
        new Song { Name = "The Fox (Ylvis)", Speed = 256, Melody = "g1 c2 ef1 f2 bf1 g2 c2 ef1 f2 g1 c1" },
        new Song { Name = "Radetzky March", Speed = 440, Melody = "e1 ef1 e2 e1 ef1 e2 e1 ef1 e2 d2 c2 e1 ef1 e2 e1 ef1 e2 e1 ef1 e2 a2 g2" },
        new Song { Name = "Steamboat Bill", Speed = 360, Melody = "ef1 e2 c2 d2 ef1 e1 ef1 e1 ef1 e1 g1" },
        new Song { Name = "Theme from “The Amazing Digital Circus”", Speed = 460, Melody = "c1 df1 d8 a2 gf2 gf2 e2 d8 a2 gf2 gf2 e1 f1 gf1" },
        new Song { Name = "Gangsta’s Paradise", Speed = 360, Melody = "f1 af2 af2 af1 g2 f2 f3 f1 f1 e2 c1 g2 af3 g1 f1 ef1 f1" },
        new Song { Name = "Barbie Girl", Speed = 260, Melody = "f1 df1 f1 bf1 gf4 ef1 c1 ef1 af1 f2 ef1 df1" },
        new Song { Name = "Whistle", Speed = 420, Melody = "a2 g2 e2 g1 e9 e2 d2 c2 d1 e1" },
        new Song { Name = "Giorno’s Theme", Speed = 540, Melody = "f6 df8 df1 ef1 e3 ef3 df2 c3 df3 ef2 f6 bf1" },
        new Song { Name = "Crab Rave", Speed = 500, Melody = "d2 bf2 g2 g1 d2 d1 a2 f2 f1 d2 d1 a2 f2 f1 c2 c2 e2 e1 f1" },
        new Song { Name = "Super Idol", Speed = 250, Melody = "g1 g1 af1 g1 f1 g1 c2 f1 ef1 c2 ef2 f1" },
        new Song { Name = "Bones", Speed = 230, Melody = "c1 ef1 c1 ef1 g2 f1 ef1 f1 f1 f1 f1 g1" },
        new Song { Name = "All Star", Speed = 210, Melody = "c2 g1 e1 e2 d1 c1 c1 f2 e1 e1 d1 d1 c1" },
        new Song { Name = "Twinkle Twinkle Little Star", Speed = 130, Melody = "c1 c1 g1 g1 a1 a1 g2 f1 f1 e1 e1 d1 d1 c1" },
        new Song { Name = "Party in the USA", Speed = 385, Melody = "c1 c1 c1 c1 g4 e2 c1 g2 g1 g2 e3 c1 d1 e1 e2 e2 c2 d1" },
        new Song { Name = "Bad Guy", Speed = 270, Melody = "c1 g2 c1 gf1 g1 gf1 ef1 c1 g2 c1 gf1 g1 gf1 ef1" },
        new Song { Name = "APT.", Speed = 300, Melody = "gf2 gf1 gf2 f1 f1 f1 c2 c2 df2 ef2 f2 f1 f2 ef1 ef1 ef1 ef2 df2 f1" },
        new Song { Name = "Toxic", Speed = 550, Melody = "c2 c2 ef1 d1 c1 d1 ef2 c2 ef1 d1 c2 g4 d4 ef2 d2 c1" },
        new Song { Name = "Blinding Lights", Speed = 340, Melody = "f4 f3 ef1 f1 g2 c2 ef3 bf1 g2 f2 ef3 bf1 g2 f2 ef2 f1" },
        new Song { Name = "Oh Little Town of Bethlehem", Speed = 250, Melody = "c2 c2 c2 d2 e1 d1 e1 f1 g2 e2 f2 e1 c1 d2 d2 c1" },
        new Song { Name = "Monkey Island", Speed = 400, Melody = "f2 f1 af1 g1 f1 ef2 f6 ef2 ef1 df1 c1 ef1 df2 df2 c1" },
        new Song { Name = "The Wild Rover", Speed = 300, Melody = "f2 f3 g1 f2 d2 c2 a2 a1 g3 a2 bf1" },
        new Song { Name = "I Can See Clearly Now The Rain Is Gone", Speed = 300, Melody = "f1 g1 a1 bf3 a1 g3 f1 a2 f2 c1" },
        new Song { Name = "Duck Tales", Speed = 375, Melody = "c2 e2 g2 a2 bf2 bf1 a2 g1 f2 f3 e5 f3 e1" },
        new Song { Name = "Circle of Life", Speed = 200, Melody = "d1 ef1 f1 f1 f1 f2 f1 g1 f2 ef6 c1 ef1 ef2 ef1 ef1 g2 f1 d1" },
        new Song { Name = "Worms", Speed = 500, Melody = "c2 d2 e4 g2 g2 g4 f2 e2 d3 c1 d2 e2 d2" },
        new Song { Name = "Always Look On The Bright Side Of Life", Speed = 540, Melody = "f4 f4 d2 f4 d2 f8 d3 c1 d3 f1" },
        new Song { Name = "99 Luftballons", Speed = 800, Melody = "g4 a2 f5 a5 g4 f2 d7 d3 bf2 bf5 bf5 bf4 bf4 a2 g2" },
        new Song { Name = "We Didn’t Start The Fire", Speed = 320, Melody = "c2 c1 c1 d2 f1 g2 a5 d1 f1 g2 a1 g2 f1 d1 c1 g2 a1 g2 f1 d1" },
        new Song { Name = "All My Loving", Speed = 350, Melody = "ef2 d2 c4 d2 ef1 f2 g5 a2 bf4 bf2 a1 g2 d1" },
        new Song { Name = "Pippi Långstrump", Speed = 400, Melody = "c2 f2 a2 f2 g4 bf1 a1 g1 f1 e2 g2 c2 e2 f4 a1" },
        new Song { Name = "I’m A Believer", Speed = 350, Melody = "c2 c2 c2 c2 d2 d2 f2 f2 e3 d1 c12 c2 c1 c3 c2 d2 d2 f2 f2 e1" },
        new Song { Name = "The Final Countdown", Speed = 400, Melody = "g1 f1 g4 c7 af1 g1 af2 g2 f1" },
        new Song { Name = "Streets of London", Speed = 350, Melody = "e2 e1 e3 g1 g2 g4 e1 d1 c6 d2 e2 e1" },
        new Song { Name = "It’s Raining Tacos", Speed = 300, Melody = "f2 f2 g1 a2 c5 a1 g1 f1 g1 e1" },
        new Song { Name = "William Tell Overture", Speed = 600, Melody = "c1 c1 c2 c1 c1 c2 c1 c1 f2 g2 a2 c1 c1 c2 c1 c1 f2 f1 a1 g2 e2 c1" },
        new Song { Name = "Lifelines", Speed = 300, Melody = "gs1 gs1 gs1 f3 cs1 cs1 cs1 c3 gs3 f3 c1 c1 c1 cs1" },
        new Song { Name = "American Pie", Speed = 280, Melody = "bf1 bf1 bf2 bf1 bf1 af2 af1 af1 g1 f1 ef1 f3 g1 f1 ef1 c1 ef1 c1 ef1 c1 c1 f1" },
        new Song { Name = "Giana Sisters", Speed = 250, Melody = "f1 c1 g1 c1 af1 c1 g1 c1 f1 c1 bf1 c1 af1 c1 g1 c1" },
        new Song { Name = "Can Somebody Tell Me Who I Am", Speed = 250, Melody = "bf1 a1 g1 f1 g2 f1 g2 a2 c1" },
        new Song { Name = "Gravity Fall", Speed = 400, Melody = "d6 e2 f8 a3 g3 a2 c8 d6 e2 f4 e4 g4 a4 g4 f1" },
    };
    readonly int? DebugFixedSong = null;

    Song SelectedSong;
    float NoteDelay;
    WaitForSeconds PrePlayDelay = new WaitForSeconds(0.2f);
    WaitForSeconds KeepSpinnersGoingFor = new WaitForSeconds(0.5f);
    WaitForSeconds PostPlayDelay = new WaitForSeconds(2f);
    int PlayerTransposeUp;
    int PlayerToInputTranspose;
    int HighestNote;
    int Stage;
    int CorrectPitch;

    private bool LockedToSubmit;
    public bool Playing = false;
    const int SpinSpeed = 300;
    const float RewindTime = 1.4f;
    private bool moduleSolved;
    static int moduleIdCounter = 1;
    public int moduleId;

    private Coroutine playSongCoroutine;
    readonly bool TwitchPlaysActive;
    readonly List<int> LogList = new List<int>();

    //For Souvenir
    public AudioClip[] PianoSounds;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        //Tell each key to go down and come up when appropriate
        foreach (KMSelectable key in PianoKeys)
        {
            key.OnInteract += delegate () { Keypress(key); return false; };
            key.OnInteractEnded += delegate () { Lift(key); };
        }
        PlayButton.OnInteract += delegate () { PlayButtonPress(); return false; };
        PlayButton.OnInteractEnded += delegate () { LiftButton(); };
        Switch.OnInteract += delegate () { FlipSwitch(); return false; };
        Module.OnActivate += delegate ()
        {
            if (TwitchPlaysActive)
            {
                Labels.SetActive(true);
            }
        };
    }

    // Use this for initialization
    void Start()
    {
        Stage = 0;

        //Choose a random song
        var selectedSongIndex = DebugFixedSong ?? UnityEngine.Random.Range(0, Songs.Length);
        SelectedSong = Songs[selectedSongIndex];

        NoteDelay = 60f / SelectedSong.Speed;
        HighestNote = SelectedSong.Notes.Max(n => n.Pitch);

        //Choose how many semitones to transpose the player up. Add 1 to the bound because the upper bound is exclusive.
        PlayerTransposeUp = UnityEngine.Random.Range(0, 12 - HighestNote);

        var minAcceptableNote = SelectedSong.Notes[0].Pitch;
        var maxAcceptableNote = minAcceptableNote + 11 - HighestNote;
        //Accidentals will always be of the form "C♯"
        Debug.Log($"[Listening Piano Keys #{moduleId}] The song is {SelectedSong.Name}, transposed up by {PlayerTransposeUp} semitones.");
        //Accidentals will always be of the form "C♯"
        Debug.Log($"[Listening Piano Keys #{moduleId}] Acceptable starting notes (inclusive): {_noteNames[minAcceptableNote][0]} - {_noteNames[maxAcceptableNote][0]}");
        Debug.Log($"[Listening Piano Keys #{moduleId}] Possible solution: {SelectedSong.Notes.Select(n => _noteNames[n.Pitch][0]).Join(" ")}");
    }

    void Keypress(KMSelectable key)
    {
        Audio.PlaySoundAtTransform(key.name.ToString(), transform);
        Vector3 currentRotation = key.transform.localEulerAngles;
        currentRotation.x = 7f;
        key.transform.localRotation = Quaternion.Euler(currentRotation);

        //If Submit mode is on and the module is not yet solved
        if (switchMovement.SwitchState & moduleSolved == false)
        {
            //If the stage is 1 and the pressed note is one that puts the rest of the song within range
            var pressedPitch = int.Parse(key.name);
            var melodyFirstPitch = SelectedSong.Notes[0].Pitch;
            if (Stage == 0 &&
                pressedPitch >= melodyFirstPitch &&
                pressedPitch <= melodyFirstPitch + 11 - HighestNote)
            {
                LockedToSubmit = true;
                LogList.Add(pressedPitch);

                //Calculate how many semitones up the defuser is playing the melody
                PlayerToInputTranspose = pressedPitch - melodyFirstPitch;
                Stage++;
                while (Stage < SelectedSong.Notes.Length && SelectedSong.Notes[Stage].Pitch - PlayerToInputTranspose == pressedPitch)
                    Stage++;

                Debug.Log($"[Listening Piano Keys #{moduleId}] First note pressed: {_noteNames[pressedPitch][0]}. With this, the correctly transposed melody is: {SelectedSong.Notes.Select(n => _noteNames[n.Pitch + PlayerToInputTranspose][0]).Join(" ")}.");
            }
            //If the first note pressed doesn't put the rest of the song within range
            else if (Stage == 0)
            {
                //Strike
                LogList.Clear();
                switchMovement.SwitchState = false;

                //For TP
                foreach (KMSelectable pianokey in PianoKeys)
                    Lift(pianokey);

                //PUT NUMBER OF THE PLAYED NOTE HERE FOR THE TABLE
                if (pressedPitch < SelectedSong.Notes[0].Pitch - PlayerTransposeUp)
                {
                    Debug.Log($"[Listening Piano Keys #{moduleId}] The lowest note of the song is {SelectedSong.Notes[0].Pitch} notes below the starting note. The played starting note ({_noteNames[pressedPitch][0]}) puts this note out of range. Striking and reselecting song.");
                }
                else
                {
                    Debug.Log($"[Listening Piano Keys #{moduleId}] The highest note of the song is {HighestNote - SelectedSong.Notes[0].Pitch} notes above the starting note. The played starting note ({_noteNames[pressedPitch][0]}) puts this note out of range. Striking and reselecting song.");
                }
                GetComponent<KMBombModule>().HandleStrike();
                Start();
            }
            else
            {
                //If not all the notes have been played
                if (Stage < SelectedSong.Notes.Length)
                {
                    CorrectPitch = SelectedSong.Notes[Stage].Pitch + PlayerToInputTranspose;

                    //If the next different note is correctly played
                    if (pressedPitch == CorrectPitch)
                    {
                        LogList.Add(pressedPitch);
                        Stage++;
                        //Skip any repeated notes
                        while (Stage < SelectedSong.Notes.Length && (SelectedSong.Notes[Stage].Pitch + PlayerToInputTranspose == pressedPitch))
                            Stage++;
                    }
                    //If the last note played was played again
                    else if (Stage > 0 && pressedPitch == SelectedSong.Notes[Stage - 1].Pitch + PlayerToInputTranspose)
                    {
                        //ignore it so that the player is allowed to safely misinterpret the number of times a note is immediately repeated
                    }
                    //If the incorrect note is played
                    else
                    {
                        //Strike
                        switchMovement.SwitchState = false;
                        LockedToSubmit = false;

                        //For TP
                        foreach (KMSelectable pianokey in PianoKeys)
                        {
                            Lift(pianokey);
                        }
                        LogList.Add(pressedPitch);
                        Debug.Log($"[Listening Piano Keys #{moduleId}] Played an incorrect note. Striking and reselecting song.\nPlayed notes (incorrect): {LogList.Join(", ")}");
                        LogList.Clear();
                        GetComponent<KMBombModule>().HandleStrike();
                        Start();
                    }
                    //If all notes have now correctly been played
                    if (Stage >= SelectedSong.Notes.Length)
                    {
                        //Solve!
                        Debug.Log($"[Listening Piano Keys #{moduleId}] Played {SelectedSong.Name} correctly. Solved.\nPlayed notes (correct): {LogList.Join(", ")}");
                        moduleSolved = true;
                        LockedToSubmit = false;
                        switchMovement.SwitchState = false;
                        foreach (KMSelectable note in PianoKeys)
                            Lift(note);

                        GetComponent<KMBombModule>().HandlePass();
                    }
                }
            }
        }
    }

    void Lift(KMSelectable key)
    {
        Vector3 currentRotation = key.transform.localEulerAngles;
        currentRotation.x = 0f;
        key.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    void LiftButton()
    {
        Vector3 currentPosition = PlayButton.transform.localPosition;
        currentPosition.y = 0.0175f;
        PlayButton.transform.localPosition = (currentPosition);
    }

    void PlayButtonPress()
    {
        PlayButton.AddInteractionPunch();
        Vector3 currentPosition = PlayButton.transform.localPosition;
        currentPosition.y = 0.015f;
        PlayButton.transform.localPosition = (currentPosition);
        if (moduleSolved && playSongCoroutine == null)
        {
            //Choose a random song
            var selectedSongIndex = UnityEngine.Random.Range(0, Songs.Length);
            SelectedSong = Songs[selectedSongIndex];

            NoteDelay = 60f / SelectedSong.Speed;
            HighestNote = SelectedSong.Notes.Max(n => n.Pitch);

            //Choose how many semitones to transpose the player up. Add 1 to the bound because the upper bound is exclusive.
            PlayerTransposeUp = UnityEngine.Random.Range(0, 12 - HighestNote);

            playSongCoroutine = StartCoroutine(Play(SelectedSong, PlayerTransposeUp));
        }
        else if (playSongCoroutine == null)
        {
            playSongCoroutine = StartCoroutine(Play(SelectedSong, PlayerTransposeUp));
        }
        else
        {
            Audio.PlaySoundAtTransform("ListeningTapeAlreadyPlaying", transform);
        }
    }

    IEnumerator Play(Song song, int transposeBy)
    {
        Audio.PlaySoundAtTransform("ListeningTapePlay", transform);

        yield return PrePlayDelay;

        Playing = true;
        StartCoroutine(SpinRoutine());
        foreach (var note in song.Notes)
        {
            Audio.PlaySoundAtTransform((note.Pitch + transposeBy).ToString(), transform);
            yield return new WaitForSeconds(NoteDelay * note.Length);
        }
        yield return KeepSpinnersGoingFor;
        Playing = false;
        Audio.PlaySoundAtTransform("ListeningTapeStop", transform);
        yield return PostPlayDelay;
        playSongCoroutine = null;
    }

    IEnumerator SpinRoutine()
    {
        while (Playing)
        {
            foreach (Transform spinner in Spinners)
            {
                Vector3 currentRotation = spinner.localRotation.eulerAngles;
                currentRotation.z += Time.deltaTime * SpinSpeed;
                spinner.localRotation = Quaternion.Euler(currentRotation);
            }
            yield return null;
        }

        yield return PrePlayDelay;

        float time = 0f;

        while (time < RewindTime)
        {
            time += Time.deltaTime;
            foreach (Transform spinner in Spinners)
            {
                Vector3 currentRotation = spinner.localRotation.eulerAngles;
                currentRotation.z -= Time.deltaTime * 2 * SpinSpeed;
                spinner.localRotation = Quaternion.Euler(currentRotation);
            }
            yield return null;
        }
    }

    void FlipSwitch()
    {
        Switch.AddInteractionPunch();
        if (!(switchMovement.SwitchState && LockedToSubmit))
        {
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            switchMovement.SwitchState = !switchMovement.SwitchState;
            Stage = 0;
        }
        else
        {
            switchMovement.JiggleLock();
        }
    }

    private readonly string TwitchHelpMessage = @"!{0} play, !{0} p [Plays the tune] | !{0} toggle, !{0} t [Toggles the switch] | !{0} c cs d ef [Plays C, C♯, D, E♭] | !{0} 1 2 3 4 5 6 7 8 9 10 11 12 [Plays all twelve keys in order from left to right]";

    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if (Regex.IsMatch(command, @"^\s*(?:play|p)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return new[] { PlayButton };
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*(?:toggle|t)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return new[] { Switch };
            yield break;
        }

        // Try to parse a space-separated list of numbers or note names
        var notes = new List<Note>();
        var regex = $@"^\s*\b([1-9]|1[0-2]|{_noteNames.Select(n => n.Join("|")).Join("|")})\b\s*";
        int notePitch;
        var curStage = Stage;
        var transposition = Stage > 0 ? PlayerToInputTranspose : (int?) null;
        while ((m = Regex.Match(command, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            // User types 1–12; indexes in ‘_noteNames’ are 0–11 so add one to make it match
            if (int.TryParse(m.Groups[1].Value, out notePitch) || (notePitch = _noteNames.IndexOf(n => n.Contains(m.Groups[1].Value, StringComparer.InvariantCultureIgnoreCase)) + 1) >= 1)
            {
                // Subtract 1 again to bring notePitch back into range 0–11
                notePitch--;
                notes.Add(transposition == null || (curStage < SelectedSong.Notes.Length && SelectedSong.Notes[curStage].Pitch + transposition.Value == notePitch)
                    // Correct according to current melody: adhere to the note length
                    ? new Note { Pitch = notePitch, Length = SelectedSong.Notes[curStage].Length }
                    // Wrong note: will strike when played, so length doesn’t matter
                    : new Note { Pitch = notePitch, Length = 1 });
                if (transposition == null)
                    transposition = notePitch - SelectedSong.Notes[curStage].Pitch;
                curStage++;
            }
            else
                yield break;
            command = command.Substring(m.Length);
        }
        // If there is any other stuff in the command, reject it because it’s not simply a list of numbers
        if (command.Length != 0 || notes.Count <= 0)
            yield break;

        yield return null;
        foreach (var note in notes)
        {
            PianoKeys[note.Pitch].OnInteract();
            yield return new WaitForSeconds(NoteDelay / 2);
            PianoKeys[note.Pitch].OnInteractEnded();
            yield return new WaitForSeconds(NoteDelay * (note.Length - 0.5f));
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        if (moduleSolved)
            yield break;
        if (!switchMovement.SwitchState)
        {
            Switch.OnInteract();
            yield return new WaitForSeconds(.5f);
        }

        if (Stage == 0)
            PlayerToInputTranspose = 0;

        while (!moduleSolved)
        {
            var note = SelectedSong.Notes[Stage];
            PianoKeys[note.Pitch + PlayerToInputTranspose].OnInteract();
            yield return new WaitForSeconds(NoteDelay / 2);
            PianoKeys[note.Pitch + PlayerToInputTranspose].OnInteractEnded();
            yield return new WaitForSeconds(NoteDelay * (note.Length - 0.5f));
        }
        foreach (KMSelectable key in PianoKeys)
            Lift(key);
    }
}