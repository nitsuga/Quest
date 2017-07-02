using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Quest.Lib.Utils
{
    public static class Serialiser
    {
        /// <summary>
        ///     Serializes object to string. Note that only public fields and properties are saved
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <returns>a string containing the XML representation of the supplied object</returns>
        public static string SerializeToString(this object obj, Type[] additionalTypes = null)
        {
            string serializedObject;

            using (var streamMemory = new MemoryStream())
            {
                Type t;
                t = obj.GetType();

                var formatter = new XmlSerializer(t, additionalTypes);
                formatter.Serialize(streamMemory, obj);

                //DataContractSerializer formatter = new DataContractSerializer(t, additionalTypes);
                //formatter.WriteObject(streamMemory, obj);

                var bytes = streamMemory.GetBuffer();
                serializedObject = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                var doc = new XmlDocument();
                doc.LoadXml(serializedObject);
                serializedObject = doc.DocumentElement.OuterXml;
            }
            return serializedObject;
        }

        /// <summary>
        ///     Same as SerialiseToString but returns an XMLDocument
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="additionalTypes"></param>
        /// <returns></returns>
        public static XmlDocument Serialize(this object obj, Type[] additionalTypes = null)
        {
            XmlDocument doc;
            string serializedObject;

            using (var streamMemory = new MemoryStream())
            {
                Type t;
                t = obj.GetType();

                var formatter = new XmlSerializer(t, additionalTypes);
                formatter.Serialize(streamMemory, obj);

                var bytes = streamMemory.GetBuffer();
                serializedObject = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                doc = new XmlDocument();
                doc.LoadXml(serializedObject);
            }
            return doc;
        }

        public static byte[] SerializeBinary(this object obj)
        {
            var binformatter = new BinaryFormatter();
            byte[] bytes;

            using (var streamMemory = new MemoryStream())
            {
                Type t;
                t = obj.GetType();

                binformatter.Serialize(streamMemory, obj);

                bytes = streamMemory.GetBuffer();
            }
            return bytes;
        }


        public static T DeserializeBinary<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            object o = null;
            var binformatter = new BinaryFormatter();

            using (var streamMemory = new MemoryStream())
            {
                streamMemory.Write(data, 0, data.Length);
                streamMemory.Position = 0;
                o = binformatter.Deserialize(streamMemory);
            }
            return (T) o;
        }

        /// <summary>
        ///     Deserialise some XML back into an object. You must supply the target object
        ///     Example:
        ///     MyClass obj = Deserialize( somestring, typeof( MyClass )) as MyClass;
        /// </summary>
        /// <param name="xml">The string that contains the object XML </param>
        /// <param name="tp">The target type to convert this xml into. </param>
        /// <returns>An object of type 'tp'</returns>
        public static object Deserialize(this string xml, Type tp, Type[] additionalTypes = null)
        {
            var formatter = CreateOverrider(tp, additionalTypes);
            return formatter.Deserialize(new StringReader(xml));
        }

        /// <summary>
        ///     Extension method for deserialising strings into object
        /// </summary>
        /// <typeparam name="T">The target class type</typeparam>
        /// <param name="xml">The string containing the xml to deserialise</param>
        /// <returns>An object of type T</returns>
        public static T Deserialize<T>(this string xml, Type[] additionalTypes = null)
        {
            var formatter = CreateOverrider(typeof(T), additionalTypes);
            return (T) formatter.Deserialize(new StringReader(xml));
        }

        /// <summary>
        ///     Return an XmlSerializer for overriding attributes
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static XmlSerializer CreateOverrider(Type tp, Type[] additionalTypes)
        {
            // Create the XmlAttributes and XmlAttributeOverrides objects.
            var attrs = new XmlAttributes();
            var xOver = new XmlAttributeOverrides();
            var xRoot = new XmlRootAttribute();

            // Set a new Namespace and ElementName for the root element.
            xRoot.Namespace = "";

            //xRoot.ElementName = "NewGroup";
            attrs.XmlRoot = xRoot;

            /* Add the XmlAttributes object to the XmlAttributeOverrides. 
               No  member name is needed because the whole class is 
               overridden. */
            xOver.Add(tp, attrs);

            var xSer = new XmlSerializer(tp, xOver, additionalTypes, xRoot, "");
            return xSer;
        }
    }
}