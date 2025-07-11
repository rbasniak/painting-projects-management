﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using rbkApiModules.Commons.Core.Abstractions;

namespace rbkApiModules.Commons.Core.UiDefinitions;

public class InputControl
{
    private readonly Type _type;

    public InputControl(string propertyName)
    {
        Name = propertyName;
    }

    public InputControl(string propertyName, Type type, RequiredAttribute requiredAttribute, MinLengthAttribute minlengAttribute,
        MaxLengthAttribute maxlengAttribute, DialogDataAttribute dialogDataAttribute)
    {
        _type = type;

        PropertyName = String.IsNullOrEmpty(dialogDataAttribute.OverridePropertyName) ? propertyName : dialogDataAttribute.OverridePropertyName;
        Required = requiredAttribute != null;
        MinLength = minlengAttribute != null ? (int?)minlengAttribute.Length : null;
        MaxLength = maxlengAttribute != null ? (int?)maxlengAttribute.Length : null;

        DataSource = dialogDataAttribute.Source != UiDefinitions.DataSource.None 
            ? new EntityReference<string> ( ((int)dialogDataAttribute.Source).ToString(), dialogDataAttribute.Source.ToString())
            : null;
        DefaultValue = dialogDataAttribute.DefaultValue;
        DependsOn = dialogDataAttribute.DependsOn;
        Group = dialogDataAttribute.Group;
        Mask = dialogDataAttribute.Mask;
        Unmask = dialogDataAttribute.Unmask;
        CharacterPattern = dialogDataAttribute.CharacterPattern;
        FileAccept = dialogDataAttribute.FileAccept;
        Name = dialogDataAttribute.Name;
        IsVisible = dialogDataAttribute.IsVisible ? null : (bool?)false;
        ExcludeFromResponse = dialogDataAttribute.ExcludeFromResponse;
        SourceName = dialogDataAttribute.SourceName;
        EntityLabelPropertyName = dialogDataAttribute.EntityLabelPropertyName;
        LinkedDisplayName = dialogDataAttribute.LinkedDisplayName;
        LinkedPropertyName = dialogDataAttribute.LinkedPropertyName;
        VisibleBasedOnInput = dialogDataAttribute.VisibleBasedOnInput;
        HiddenBasedOnInput = dialogDataAttribute.HiddenBasedOnInput;

        var control = dialogDataAttribute.ForcedType != DialogControlTypes.Default ? dialogDataAttribute.ForcedType : GetControlType();

        ControlType = new EntityReference<string>( ((int)control).ToString(), control.ToString() );

        if (control == DialogControlTypes.DropDown || control == DialogControlTypes.MultiSelect || control == DialogControlTypes.LinkedDropDown)
        {
            PropertyName = FixDropDownName(propertyName);
            ShowFilter = dialogDataAttribute.ShowFilter;
            FilterMatchMode = dialogDataAttribute.FilterMatchMode;
        }

        if (control == DialogControlTypes.TextArea)
        {
            TextAreaRows = dialogDataAttribute.TextAreaRows > 0 ? dialogDataAttribute.TextAreaRows : 5;
        }

        // -- 

        Type enumType = null;

        if (_type.IsEnum)
        {
            Required = true;
            enumType = _type;
        }
        else if (Nullable.GetUnderlyingType(_type)?.IsEnum == true)
        {
            Required = false;
            enumType = Nullable.GetUnderlyingType(_type);
        }

        if (enumType != null)
        {
            Data = EnumToSimpleNamedList(enumType).OrderBy(x => x.Name).ToList();
        }

        // -- 


        // --
        // The Attribute doesn't accept nullable properties, so this is handled
        // here to doesn't send default values to the front
        if (ExcludeFromResponse == false)
        {
            ExcludeFromResponse = null;
        }

        if (TextAreaRows == 0)
        {
            TextAreaRows = null;
        }
        // --
    }

    public EntityReference<string> ControlType { get; set; }
    public EntityReference<string> DataSource { get; set; }
    public string SourceName { get; set; }
    public string PropertyName { get; set; }

    public string Name { get; set; }
    public object DefaultValue { get; set; }
    public string Group { get; set; }
    public bool? IsVisible { get; set; }
    public bool? ExcludeFromResponse { get; set; }

    public string DependsOn { get; set; }

    public int? TextAreaRows { get; set; }

    public string Mask { get; set; }
    public bool? Unmask { get; set; }
    public string CharacterPattern { get; set; }

    public string FileAccept { get; set; }

    public bool? ShowFilter { get; set; }
    public string FilterMatchMode { get; set; }

    public bool Required { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public List<EntityReference<object>> Data { get; set; }

    public string EntityLabelPropertyName { get; set; }

    public string LinkedDisplayName { get; set; }
    public string LinkedPropertyName { get; set; }
    public string VisibleBasedOnInput { get; set; }
    public string HiddenBasedOnInput { get; set; }

    private DialogControlTypes GetControlType()
    {
        if (_type.FullName == typeof(String).FullName)
        {
            if (MaxLength.HasValue && MaxLength.Value <= 512)
            {
                return DialogControlTypes.Text;
            }
            else if (MaxLength.HasValue && MaxLength.Value > 512)
            {
                return DialogControlTypes.TextArea;
            }
            else
            {
                return DialogControlTypes.Text;
            }
        }
        else if (_type.FullName == typeof(Boolean).FullName || _type.FullName == typeof(Boolean?).FullName)
        {
            return DialogControlTypes.CheckBox;
        }
        else if (_type.FullName == typeof(Int32).FullName || _type.FullName == typeof(Int64).FullName ||
                 _type.FullName == typeof(Int32?).FullName || _type.FullName == typeof(Int64?).FullName)
        {
            return DialogControlTypes.Number;
        }
        else if (_type.FullName == typeof(Single).FullName || _type.FullName == typeof(Single?).FullName ||
                 _type.FullName == typeof(Double).FullName || _type.FullName == typeof(Double?).FullName ||
                 _type.FullName == typeof(Decimal).FullName || _type.FullName == typeof(Decimal?).FullName)
        {
            return DialogControlTypes.Decimal;
        }
        else if (_type.FullName == typeof(DateTime).FullName || _type.FullName == typeof(DateTime?).FullName)
        {
            return DialogControlTypes.Calendar;
        }
        else if (typeof(BaseEntity).IsAssignableFrom(_type))
        {
            PropertyName = FixDropDownName(PropertyName);
            return DialogControlTypes.DropDown;
        }
        else if (_type.IsEnum)
        {
            return DialogControlTypes.DropDown;
        }
        else if (Nullable.GetUnderlyingType(_type)?.IsEnum == true)
        {
            return DialogControlTypes.DropDown;
        }
        else if (_type.FullName == typeof(string[]).FullName || _type.FullName == typeof(List<string>).FullName)
        {
            return DialogControlTypes.List;
        }
        else if (_type.FullName.StartsWith("System.Collections.Generic.List`1") || _type.FullName.StartsWith("System.Collections.Generic.IEnumerable`1"))
        {
            return DialogControlTypes.MultiSelect;
        }
        else if (_type.FullName == typeof(Guid).FullName || _type.FullName == typeof(Guid?).FullName)
        {
            return DialogControlTypes.Text;
        }
        else
        {
            throw new NotSupportedException("Type not supported: " + _type.FullName);
        }
    }

    private string FixDropDownName(string propertyName)
    {
        return propertyName.EndsWith("Id") ? propertyName.Substring(0, propertyName.Length - 2) : propertyName;
    }

    private List<EntityReference<object>> EnumToSimpleNamedList(Type type)
    {
        var results = new List<EntityReference<object>>();
        var names = Enum.GetNames(type);
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var field = type.GetField(name);
            var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
            var id = (int)Enum.Parse(type, name);

            if (fds != null)
            {
                results.Add(new EntityReference<object>(id, (fds as DescriptionAttribute).Description ));
            }
            else
            {
                results.Add(new EntityReference<object>(id, field.Name ));
            }
        }

        return results;
    }
}

