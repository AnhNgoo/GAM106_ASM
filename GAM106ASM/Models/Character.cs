using System;
using System.Collections.Generic;

namespace GAM106ASM.Models;

public partial class Character
{
    public int CharacterId { get; set; }

    public int PlayerId { get; set; }

    public string CharacterName { get; set; } = null!;

    public string? Sex { get; set; }

    public string? Skin { get; set; }

    public virtual Player Player { get; set; } = null!;
}
