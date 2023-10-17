using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ScriptPortal.Vegas;

public class EntryPoint
{
    private Vegas vegas;
    private CheckBox chkFlip;
    private CheckBox chkFill;
    private CheckBox chkScale;
    private RadioButton radOutIn;
    private RadioButton radInOutSharp;
    private RadioButton radInOutSmooth;
    private Button btnConfirm;
    private Form frmPopup;
    private Form frmScale;

    public void FromVegas(Vegas vegas)
    {
        this.vegas = vegas;

        frmPopup = new Form
        {
            Text = "Javier's Useful Scripts",
            Width = 360,
            Height = 240,
            AutoScaleMode = AutoScaleMode.Font
        };

        chkFlip = new CheckBox
        {
            Text = "Flip Even Video Events",
            Left = 10,
            Top = 30,
            Checked = false,
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        frmPopup.Controls.Add(chkFlip);

        chkFill = new CheckBox
        {
            Text = "Fill Gaps Between Video Events",
            Left = 10,
            Top = 70,
            Checked = false,
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        frmPopup.Controls.Add(chkFill);

        chkScale = new CheckBox
        {
            Text = "Scale Keyframes with Center Fixed (miniOTOMAD)",
            Left = 10,
            Top = 110,
            Checked = false,
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        frmPopup.Controls.Add(chkScale);

        btnConfirm = new Button
        {
            Text = "Confirm",
            Left = 10,
            Top = 150,
            Font = new System.Drawing.Font("Arial", 8)
        };
        btnConfirm.Click += ConfirmButtonClick;
        btnConfirm.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        frmPopup.Controls.Add(btnConfirm);

        frmPopup.ShowDialog();

        frmScale = new Form
        {
            Text = "Scale Keyframes",
            Width = 380,
            Height = 240,
            AutoScaleMode = AutoScaleMode.Font
        };

        // Create radio buttons for scaling options
        radOutIn = new RadioButton
        {
            Text = "Scale Out of Bounds to In Bounds (reccomended for box visuals)",
            Left = 10,
            Top = 40,
            Checked = false,
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        frmScale.Controls.Add(radOutIn);

        radInOutSharp = new RadioButton
        {
            Text = "Scale In Bounds to Out of Bounds (Sharp Transition)",
            Left = 10,
            Top = 70,
            Checked = false,
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        frmScale.Controls.Add(radInOutSharp);

        radInOutSmooth = new RadioButton
        {
            Text = "Scale In Bounds to Out of Bounds (Smooth Transition) (reccomended for chorus/freestyle)",
            Left = 10,
            Top = 100,
            Checked = false,
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        frmScale.Controls.Add(radInOutSmooth);

        // Create a confirm button for the scale form
        Button btnScaleConfirm = new Button
        {
            Text = "Confirm",
            Left = 10,
            Top = 130,
            Font = new System.Drawing.Font("Arial", 8)
        };
        btnScaleConfirm.Click += ScaleConfirmButtonClick;
        btnScaleConfirm.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        frmScale.Controls.Add(btnScaleConfirm);
        frmScale.ShowDialog();
    }

    private void ScaleConfirmButtonClick(object sender, EventArgs e)
    {
        if (radOutIn.Checked)
        {
            PerformScaleAction(0);
        }

        if (radInOutSharp.Checked)
        {
            PerformScaleAction(1);
        }

        if (radInOutSmooth.Checked)
        {
            PerformScaleAction(2);
        }

        frmScale.Close();
    }

    private void ConfirmButtonClick(object sender, EventArgs e)
    {
        if (chkFill.Checked) PerformFillAction();
        if (chkFlip.Checked) PerformFlipAction();
        if (chkScale.Checked)
        {
            frmPopup.Close();
        }
        frmPopup.Close();
    }

    private void PerformFlipAction()
    {
        bool flip = false;
        List<VideoTrack> Tracks = new List<VideoTrack>();
        foreach (Track Track in vegas.Project.Tracks) { if (Track.Selected && Track.IsVideo()) { Tracks.Add((VideoTrack)Track); } }
        foreach (VideoTrack Track in Tracks)
        {
            foreach (VideoEvent Event in Track.Events)
            {
                if (flip)
                {
                    foreach (VideoMotionKeyframe Keyframe in Event.VideoMotion.Keyframes)
                    {
                        VideoMotionBounds Bounds = new VideoMotionBounds(Keyframe.TopRight, Keyframe.TopLeft, Keyframe.BottomLeft, Keyframe.BottomLeft);
                        Keyframe.Bounds = Bounds;
                    }
                }
                flip = !flip;
            }
        }
    }

    private void PerformFillAction()
    {
        List<VideoTrack> Tracks = new List<VideoTrack>();
        foreach (Track Track in vegas.Project.Tracks) { if (Track.Selected && Track.IsVideo()) { Tracks.Add((VideoTrack)Track); } }
        foreach (VideoTrack Track in Tracks)
        {
            List<TrackEvent> events = new List<TrackEvent>(Track.Events);
            for (int i = 0; i < events.Count; i++)
            {
                TrackEvent Event = events[i];
                if (i < events.Count - 1) { Event.Length = Timecode.FromMilliseconds(events[i + 1].Start.ToMilliseconds() - Event.Start.ToMilliseconds()); }
            }
        }
    }

    private void PerformScaleAction(int Mode)
    {
        List<VideoTrack> Tracks = new List<VideoTrack>();
        foreach (Track Track in vegas.Project.Tracks) { if (Track.Selected && Track.IsVideo()) { Tracks.Add((VideoTrack)Track); } }

        foreach (VideoTrack Track in Tracks)
        {
            foreach (VideoEvent Event in Track.Events)
            {
                float ScaleFactor = 1.125F;
                float SmoothnessFactor = 2F;

                VideoMotionKeyframe First = Event.VideoMotion.Keyframes[0];
                First.Type = VideoKeyframeType.Smooth;

                if (Event.Length.ToMilliseconds() < 60 / vegas.Project.Ruler.BeatsPerMinute * 1000) { SmoothnessFactor = 1.5F; }
                VideoMotionKeyframe Last = new VideoMotionKeyframe(Timecode.FromMilliseconds(Event.Length.ToMilliseconds() / SmoothnessFactor));

                Event.VideoMotion.Keyframes.Add(Last);
                Last.Type = VideoKeyframeType.Sharp;

                VideoMotionBounds Bounds = First.Bounds;

                float CenterX = (Bounds.TopLeft.X + Bounds.TopRight.X) / 2;
                float CenterY = (Bounds.TopLeft.Y + Bounds.BottomLeft.Y) / 2;

                First.Bounds = Bounds;

                if (Mode.Equals(0))
                {
                    // Out of bounds, to in bounds.
                    Bounds.TopLeft.X = CenterX + (Bounds.TopLeft.X - CenterX / 2) * ScaleFactor;
                    Bounds.TopLeft.Y = CenterY + (Bounds.TopLeft.Y - CenterY / 2) * ScaleFactor;
                    Bounds.TopRight.X = CenterX + (Bounds.TopRight.X - CenterX / 2) * ScaleFactor;
                    Bounds.TopRight.Y = CenterY + (Bounds.TopRight.Y - CenterY / 2) * ScaleFactor;
                    Bounds.BottomLeft.X = CenterX + (Bounds.BottomLeft.X - CenterX / 2) * ScaleFactor;
                    Bounds.BottomLeft.Y = CenterY + (Bounds.BottomLeft.Y - CenterY / 2) * ScaleFactor;
                    Bounds.BottomRight.X = CenterX + (Bounds.BottomRight.X - CenterX / 2) * ScaleFactor;
                    Bounds.BottomRight.Y = CenterY + (Bounds.BottomRight.Y - CenterY / 2) * ScaleFactor;
                }

                if (Mode.Equals(1))
                {
                    // From inner bounds, to in bounds (sharp).
                    Bounds.TopLeft.X = (Bounds.TopLeft.X - CenterX) / (ScaleFactor * 1.375F) + CenterX;
                    Bounds.TopLeft.Y = (Bounds.TopLeft.Y - CenterY) / (ScaleFactor * 1.375F) + CenterY;
                    Bounds.TopRight.X = (Bounds.TopRight.X - CenterX) / (ScaleFactor * 1.375F) + CenterX;
                    Bounds.TopRight.Y = (Bounds.TopRight.Y - CenterY) / (ScaleFactor * 1.375F) + CenterY;
                    Bounds.BottomLeft.X = (Bounds.BottomLeft.X - CenterX) / (ScaleFactor * 1.375F) + CenterX;
                    Bounds.BottomLeft.Y = (Bounds.BottomLeft.Y - CenterY) / (ScaleFactor * 1.375F) + CenterY;
                    Bounds.BottomRight.X = (Bounds.BottomRight.X - CenterX) / (ScaleFactor * 1.375F) + CenterX;
                    Bounds.BottomRight.Y = (Bounds.BottomRight.Y - CenterY) / (ScaleFactor * 1.375F) + CenterY;
                }

                if (Mode.Equals(2))
                {
                    // From inner bounds, to in bounds (smooth).
                    Bounds.TopLeft.X = (Bounds.TopLeft.X - CenterX) / (ScaleFactor) + CenterX;
                    Bounds.TopLeft.Y = (Bounds.TopLeft.Y - CenterY) / (ScaleFactor) + CenterY;
                    Bounds.TopRight.X = (Bounds.TopRight.X - CenterX) / (ScaleFactor) + CenterX;
                    Bounds.TopRight.Y = (Bounds.TopRight.Y - CenterY) / (ScaleFactor) + CenterY;
                    Bounds.BottomLeft.X = (Bounds.BottomLeft.X - CenterX) / (ScaleFactor) + CenterX;
                    Bounds.BottomLeft.Y = (Bounds.BottomLeft.Y - CenterY) / (ScaleFactor) + CenterY;
                    Bounds.BottomRight.X = (Bounds.BottomRight.X - CenterX) / (ScaleFactor) + CenterX;
                    Bounds.BottomRight.Y = (Bounds.BottomRight.Y - CenterY) / (ScaleFactor) + CenterY;
                }

                First.Bounds = Bounds;
            }
        }
    }
}
