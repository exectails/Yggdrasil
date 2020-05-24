using System.Linq.Expressions;
using System.Reflection;

namespace Yggdrasil.Util
{
	// https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/

	/// <summary>
	/// Creates an object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="args"></param>
	/// <returns></returns>
	public delegate T ObjectActivatorFunc<T>(params object[] args);

	/// <summary>
	/// Generator for dedicated object activators.
	/// </summary>
	public static class ObjectActivator
	{
		/// <summary>
		/// Creates an activator function that creates objects using the given
		/// constructor.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ctor"></param>
		/// <returns></returns>
		public static ObjectActivatorFunc<T> GetActivator<T>(ConstructorInfo ctor)
		{
			var type = ctor.DeclaringType;
			var paramsInfo = ctor.GetParameters();

			// create a single param of type object[]
			var param = Expression.Parameter(typeof(object[]), "args");

			var argsExp = new Expression[paramsInfo.Length];

			// pick each arg from the params array 
			// and create a typed expression of them
			for (var i = 0; i < paramsInfo.Length; i++)
			{
				var index = Expression.Constant(i);
				var paramType = paramsInfo[i].ParameterType;

				var paramAccessorExp = Expression.ArrayIndex(param, index);
				var paramCastExp = Expression.Convert(paramAccessorExp, paramType);

				argsExp[i] = paramCastExp;
			}

			// make a NewExpression that calls the
			// ctor with the args we just created
			var newExp = Expression.New(ctor, argsExp);

			// create a lambda with the New
			// expression as body and our param object[] as arg
			var lambda = Expression.Lambda(typeof(ObjectActivatorFunc<T>), newExp, param);

			// compile it
			var compiled = (ObjectActivatorFunc<T>)lambda.Compile();
			return compiled;
		}
	}
}
