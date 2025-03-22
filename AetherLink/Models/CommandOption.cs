using System;
using System.Collections.Generic;
using Discord;

namespace AetherLink.Models;
#nullable disable
public class CommandOption
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ApplicationCommandOptionType Type { get; set; }
    public bool IsRequired { get; set; }
    public bool IsAutoFill { get; set; } = false;
}