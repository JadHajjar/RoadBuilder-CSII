using Colossal.UI.Binding;

using System.Collections.Generic;
using System.Linq;

namespace RoadBuilder.Domain.UI
{
	public class OptionSectionUIEntry : IJsonWritable
	{
		private OptionItemUIEntry[] _options;

		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsToggle { get; set; }
		public bool IsButton { get; set; }
		public bool IsCheckbox { get; set; }
		public IEnumerable<OptionItemUIEntry> Options
		{
			get => _options;
			set => _options = value.Where(x => !x.Hidden).ToArray();
		}

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("name");
			writer.Write(Name);

			writer.PropertyName("isToggle");
			writer.Write(IsToggle);

			writer.PropertyName("isButton");
			writer.Write(IsButton);

			writer.PropertyName("isCheckbox");
			writer.Write(IsCheckbox);

			writer.PropertyName("options");
			writer.ArrayBegin(_options.Length);
			for (var i = 0; i < _options.Length; i++)
			{
				_options[i].Write(writer);
			}

			writer.ArrayEnd();

			writer.TypeEnd();
		}
	}
}
