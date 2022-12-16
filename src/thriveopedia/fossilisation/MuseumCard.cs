﻿using Godot;

/// <summary>
///   Card displaying a fossilised species in the Thriveopedia museum.
/// </summary>
public class MuseumCard : Button
{
    [Export]
    public NodePath SpeciesNameLabelPath = null!;

    [Export]
    public NodePath SpeciesPreviewPath = null!;

    [Export]
    public NodePath DeleteButtonPath = null!;

    [Export]
    public NodePath DeleteConfirmationDialogPath = null!;

    private Label? speciesNameLabel;
    private TextureRect? speciesPreview;

    private Species? savedSpecies;
    private Image? fossilPreviewImage;

    private TextureButton? deleteButton;

    private CustomConfirmationDialog deleteConfirmationDialog = null!;

    [Signal]
    public delegate void OnSpeciesSelected(MuseumCard card);

    [Signal]
    public delegate void OnSpeciesDeleted(MuseumCard card);

    /// <summary>
    ///   The fossilised species associated with this card.
    /// </summary>
    public Species? SavedSpecies
    {
        get => savedSpecies;
        set
        {
            savedSpecies = value;
            UpdateSpeciesName();
        }
    }

    /// <summary>
    ///   If the fossil file had a preview image, that is set here and is used used to preview the species
    /// </summary>
    public Image? FossilPreviewImage
    {
        get => fossilPreviewImage;
        set
        {
            if (value == fossilPreviewImage)
                return;

            fossilPreviewImage = value;
            UpdatePreviewImage();
        }
    }

    public string? FossilName { get; set; }

    private bool wasPressed;

    public override void _Ready()
    {
        base._Ready();

        speciesPreview = GetNode<TextureRect>(SpeciesPreviewPath);
        speciesNameLabel = GetNode<Label>(SpeciesNameLabelPath);
        deleteButton = GetNode<TextureButton>(DeleteButtonPath);
        deleteConfirmationDialog = GetNode<CustomConfirmationDialog>(DeleteConfirmationDialogPath);

        UpdateSpeciesName();
        UpdatePreviewImage();
    }

    private void UpdateSpeciesName()
    {
        if (SavedSpecies == null || speciesNameLabel == null)
            return;

        speciesNameLabel.Text = SavedSpecies.FormattedName;
    }

    private void UpdatePreviewImage()
    {
        if (speciesPreview == null)
            return;

        if (FossilPreviewImage != null)
        {
            var imageTexture = new ImageTexture();

            // Could add filter and mipmap flags here if the preview images look too bad at small sizes, but that
            // would presumably make this take more time, so maybe then this shouldn't be done in a blocking way here
            // and instead using ResourceManager
            imageTexture.CreateFromImage(FossilPreviewImage);

            speciesPreview.Texture = imageTexture;
        }
    }

    private void OnPressed()
    {
        // Make sure clicking doesn't pass through from the delete button
        if(deleteButton?.Pressed ?? false) {
            Pressed = wasPressed;
            return;
        }
        wasPressed = Pressed;

        GUICommon.Instance.PlayButtonPressSound();
        EmitSignal(nameof(OnSpeciesSelected), this);
    }

    private void OnMouseEnter()
    {
        if(deleteButton != null)
            deleteButton.Visible = true;

        GUICommon.Instance.Tween.InterpolateProperty(speciesPreview, "modulate", null, Colors.Gray, 0.5f);
        GUICommon.Instance.Tween.Start();
    }

    private void OnMouseExit()
    {
        if(deleteButton != null)
            deleteButton.Visible = false;

        GUICommon.Instance.Tween.InterpolateProperty(speciesPreview, "modulate", null, Colors.White, 0.5f);
        GUICommon.Instance.Tween.Start();
    }

    private void OnDeletePressed()
    {
        GUICommon.Instance.PlayButtonPressSound();

        deleteConfirmationDialog.DialogText = TranslationServer.Translate("DELETE_FOSSIL_CONFIRMATION");
        deleteConfirmationDialog.PopupCenteredShrink();
    }

    private void OnDeleteConfirmPressed()
    {
        EmitSignal(nameof(OnSpeciesDeleted), this);
    }
}
