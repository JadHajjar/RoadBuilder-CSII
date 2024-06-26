using Colossal.UI.Binding;

using Game.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Unity.Entities;

using UnityEngine;

namespace RoadBuilder.Systems.UI
{
	public abstract partial class ExtendedUISystemBase : UISystemBase
	{
		public ValueBindingHelper<T> CreateBinding<T>(string key, T initialValue)
		{
			var helper = new ValueBindingHelper<T>(new(Mod.Id, key, initialValue, new GenericUIWriter<T>()));

			AddBinding(helper.Binding);

			return helper;
		}

		public ValueBindingHelper<T> CreateBinding<T>(string key, string setterKey, T initialValue, Action<T> updateCallBack = null)
		{
			var helper = new ValueBindingHelper<T>(new(Mod.Id, key, initialValue, new GenericUIWriter<T>()), updateCallBack);
			var trigger = new TriggerBinding<T>(Mod.Id, setterKey, helper.UpdateCallback, new GenericUIReader<T>());

			AddBinding(helper.Binding);
			AddBinding(trigger);

			return helper;
		}

		public GetterValueBinding<T> CreateBinding<T>(string key, Func<T> getterFunc)
		{
			var binding = new GetterValueBinding<T>(Mod.Id, key, getterFunc, new GenericUIWriter<T>());

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding CreateTrigger(string key, Action action)
		{
			var binding = new TriggerBinding(Mod.Id, key, action);

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1> CreateTrigger<T1>(string key, Action<T1> action)
		{
			var binding = new TriggerBinding<T1>(Mod.Id, key, action, new GenericUIReader<T1>());

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1, T2> CreateTrigger<T1, T2>(string key, Action<T1, T2> action)
		{
			var binding = new TriggerBinding<T1, T2>(Mod.Id, key, action, new GenericUIReader<T1>(), new GenericUIReader<T2>());

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1, T2, T3> CreateTrigger<T1, T2, T3>(string key, Action<T1, T2, T3> action)
		{
			var binding = new TriggerBinding<T1, T2, T3>(Mod.Id, key, action, new GenericUIReader<T1>(), new GenericUIReader<T2>(), new GenericUIReader<T3>());

			AddBinding(binding);

			return binding;
		}

		public TriggerBinding<T1, T2, T3, T4> CreateTrigger<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> action)
		{
			var binding = new TriggerBinding<T1, T2, T3, T4>(Mod.Id, key, action, new GenericUIReader<T1>(), new GenericUIReader<T2>(), new GenericUIReader<T3>(), new GenericUIReader<T4>());

			AddBinding(binding);

			return binding;
		}
	}

	public class ValueBindingHelper<T>
	{
		private readonly Action<T> _updateCallBack;

		public ValueBinding<T> Binding { get; }

		public T Value { get => Binding.value; set => Binding.Update(value); }

		public ValueBindingHelper(ValueBinding<T> binding, Action<T> updateCallBack = null)
		{
			Binding = binding;
			_updateCallBack = updateCallBack;
		}

		public void UpdateCallback(T value)
		{
			Binding.Update(value);
			_updateCallBack?.Invoke(value);
		}

		public static implicit operator T(ValueBindingHelper<T> helper)
		{
			return helper.Binding.value;
		}
	}

	public class GenericUIWriter<T> : IWriter<T>
	{
		public void Write(IJsonWriter writer, T value)
		{
			WriteGeneric(writer, value);
		}

		private static void WriteObject(IJsonWriter writer, Type type, object obj)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			writer.TypeBegin(type.FullName);

			foreach (var propertyInfo in properties)
			{
				writer.PropertyName(propertyInfo.Name);
				WriteGeneric(writer, propertyInfo.GetValue(obj));
			}

			foreach (var fieldInfo in fields)
			{
				writer.PropertyName(fieldInfo.Name);
				WriteGeneric(writer, fieldInfo.GetValue(obj));
			}

			writer.TypeEnd();
		}

		private static void WriteGeneric(IJsonWriter writer, object obj)
		{
			if (obj == null)
			{
				writer.WriteNull();
				return;
			}

			if (obj is IJsonWritable jsonWritable)
			{
				jsonWritable.Write(writer);
				return;
			}

			if (obj is int @int)
			{
				writer.Write(@int);
				return;
			}

			if (obj is bool @bool)
			{
				writer.Write(@bool);
				return;
			}

			if (obj is uint @uint)
			{
				writer.Write(@uint);
				return;
			}

			if (obj is float @float)
			{
				writer.Write(@float);
				return;
			}

			if (obj is double @double)
			{
				writer.Write(@double);
				return;
			}

			if (obj is string @string)
			{
				writer.Write(@string);
				return;
			}

			if (obj is Enum @enum)
			{
				writer.Write(Convert.ToInt32(@enum));
				return;
			}

			if (obj is Entity entity)
			{
				writer.Write(entity);
				return;
			}

			if (obj is Color color)
			{
				writer.Write(color);
				return;
			}

			if (obj is Array array)
			{
				WriteArray(writer, array);
				return;
			}

			if (obj is IEnumerable objects)
			{
				WriteEnumerable(writer, objects);
				return;
			}

			WriteObject(writer, obj.GetType(), obj);
		}

		private static void WriteArray(IJsonWriter writer, Array array)
		{
			writer.ArrayBegin(array.Length);

			for (var i = 0; i < array.Length; i++)
			{
				WriteGeneric(writer, array.GetValue(i));
			}

			writer.ArrayEnd();
		}

		private static void WriteEnumerable(IJsonWriter writer, object obj)
		{
			var list = new List<object>();

			foreach (var item in obj as IEnumerable)
			{
				list.Add(item);
			}

			writer.ArrayBegin(list.Count);

			foreach (var item in list)
			{
				WriteGeneric(writer, item);
			}

			writer.ArrayEnd();
		}
	}

	public class GenericUIReader<T> : IReader<T>
	{
		public void Read(IJsonReader reader, out T value)
		{
			value = (T)ReadGeneric(reader, typeof(T));
		}

		private static object ReadGeneric(IJsonReader reader, Type type)
		{
			if (type.IsAssignableFrom(typeof(IJsonReadable)))
			{
				var value = (IJsonReadable)Activator.CreateInstance(type);

				value.Read(reader);

				return value;
			}

			if (type == typeof(int))
			{
				reader.Read(out int val);

				return val;
			}

			if (type == typeof(bool))
			{
				reader.Read(out bool val);

				return val;
			}

			if (type == typeof(uint))
			{
				reader.Read(out uint val);

				return val;
			}

			if (type == typeof(float))
			{
				reader.Read(out float val);

				return val;
			}

			if (type == typeof(double))
			{
				reader.Read(out double val);

				return val;
			}

			if (type == typeof(string))
			{
				reader.Read(out string val);

				return val;
			}

			if (type == typeof(Enum))
			{
				reader.Read(out int val);

				return val;
			}

			if (type == typeof(Entity))
			{
				reader.Read(out Entity val);

				return val;
			}

			if (type == typeof(Color))
			{
				reader.Read(out Color val);

				return val;
			}

			if (type == typeof(Array))
			{
				var length = (int)reader.ReadArrayBegin();
				var array = (Array)Activator.CreateInstance(type.GetElementType(), length);

				for (var i = 0; i < length; i++)
				{
					reader.ReadArrayElement((ulong)i);

					array.SetValue(ReadGeneric(reader, type.GetElementType()), i);
				}

				reader.ReadArrayEnd();

				return array;
			}

			if (type == typeof(IEnumerable))
			{
				var length = reader.ReadArrayBegin();
				var genericListType = typeof(List<>).MakeGenericType(type.GetElementType());
				var genericList = (IList)Activator.CreateInstance(genericListType);

				for (var i = 0ul; i < length; i++)
				{
					reader.ReadArrayElement(i);

					genericList.Add(ReadGeneric(reader, type.GetElementType()));
				}

				reader.ReadArrayEnd();

				return genericList;
			}

			return ReadObject(reader, type);
		}

		private static object ReadObject(IJsonReader reader, Type type)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var obj = Activator.CreateInstance(type);

			foreach (var propertyInfo in properties)
			{
				if (reader.ReadProperty(propertyInfo.Name))
				{
					propertyInfo.SetValue(obj, ReadGeneric(reader, propertyInfo.PropertyType));
				}
			}

			foreach (var fieldInfo in fields)
			{
				if (reader.ReadProperty(fieldInfo.Name))
				{
					fieldInfo.SetValue(obj, ReadGeneric(reader, fieldInfo.FieldType));
				}
			}

			return obj;
		}
	}
}
