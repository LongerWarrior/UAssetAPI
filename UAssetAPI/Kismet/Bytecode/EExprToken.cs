namespace UAssetAPI.Kismet.Bytecode
{
    /// <summary>
    /// Evaluatable expression item types.
    /// </summary>
    public enum EExprToken
	{
		/// <summary>A local variable.</summary>
		EX_LocalVariable = 0x1a,
		/// <summary>An object variable.</summary>
		EX_InstanceVariable = 0x1b,
		/// <summary>Default variable for a class context.</summary>
		EX_DefaultVariable = 0x1c,
		/// <summary>Return from function.</summary>
		EX_Return = 0x05,
		/// <summary>Goto a local address in code.</summary>
		EX_Jump = 0x07,
		/// <summary>Goto if not expression.</summary>
		EX_JumpIfNot = 0x08,
		/// <summary>Assertion.</summary>
		EX_Assert = 0x03,
		/// <summary>No operation.</summary>
		EX_Nothing = 0x01,
		/// <summary>Assign an arbitrary size value to a variable.</summary>
		EX_Let = 0x34,
		/// <summary>Class default object context.</summary>
		EX_ClassContext = 0x0a,
		/// <summary>Metaclass cast.</summary>
		EX_MetaCast = 0x17,
		/// <summary>Let boolean variable.</summary>
		EX_LetBool = 0x35,
		/// <summary>end of default value for optional function parameter</summary>
		EX_EndParmValue = 0x0b,
		/// <summary>End of function call parameters.</summary>
		EX_EndFunctionParms = 0x0c,
		/// <summary>Self object.</summary>
		EX_Self = 0x0d,
		/// <summary>Skippable expression.</summary>
		EX_Skip = 0x0e,
		/// <summary>Call a function through an object context.</summary>
		EX_Context = 0x0f,
		/// <summary>Call a function through an object context (can fail silently if the context is NULL; only generated for functions that don't have output or return values).</summary>
		EX_Context_FailSilent = 0x10,
		/// <summary>A function call with parameters.</summary>
		EX_VirtualFunction = 0x11,
		/// <summary>A prebound function call with parameters.</summary>
		EX_FinalFunction = 0x12,
		/// <summary>Int constant.</summary>
		EX_IntConst = 0x2a,
		/// <summary>Floating point constant.</summary>
		EX_FloatConst = 0x2b,
		/// <summary>String constant.</summary>
		EX_StringConst = 0x2c,
		/// <summary>An object constant.</summary>
		EX_ObjectConst = 0x2d,
		/// <summary>A name constant.</summary>
		EX_NameConst = 0x2e,
		/// <summary>A rotation constant.</summary>
		EX_RotationConst = 0x2f,
		/// <summary>A vector constant.</summary>
		EX_VectorConst = 0x30,
		/// <summary>A byte constant.</summary>
		EX_ByteConst = 0x31,
		/// <summary>Zero.</summary>
		EX_IntZero = 0x1d,
		/// <summary>One.</summary>
		EX_IntOne = 0x1e,
		/// <summary>Bool True.</summary>
		EX_True = 0x1f,
		/// <summary>Bool False.</summary>
		EX_False = 0x20,
		/// <summary>FText constant</summary>
		EX_TextConst = 0x32,
		/// <summary>NoObject.</summary>
		EX_NoObject = 0x13,
		/// <summary>A transform constant</summary>
		EX_TransformConst = 0x24,
		/// <summary>Int constant that requires 1 byte.</summary>
		EX_IntConstByte = 0x29,
		/// <summary>A null interface (similar to EX_NoObject, but for interfaces)</summary>
		EX_NoInterface = 0x14,
		/// <summary>Safe dynamic class casting.</summary>
		EX_DynamicCast = 0x16,
		/// <summary>An arbitrary UStruct constant</summary>
		EX_StructConst = 0x22,
		/// <summary>End of UStruct constant</summary>
		EX_EndStructConst = 0x23,
		/// <summary>Set the value of arbitrary array</summary>
		EX_SetArray = 0x3b,
		EX_EndArray = 0x3c,
		/// <summary>FProperty constant.</summary>
		EX_PropertyConst = 0x25,
		/// <summary>Unicode string constant.</summary>
		EX_UnicodeStringConst = 0x26,
		/// <summary>64-bit integer constant.</summary>
		EX_Int64Const = 0x27,
		/// <summary>64-bit unsigned integer constant.</summary>
		EX_UInt64Const = 0x28,
		/// <summary>Double-precision floating point constant.</summary>
		EX_DoubleConst = 0x99,
		/// <summary>A casting operator for primitives which reads the type as the subsequent byte</summary>
		EX_PrimitiveCast = 0x18,
		EX_SetSet = 0x3d,
		EX_EndSet = 0x3e,
		EX_SetMap = 0x3f,
		EX_EndMap = 0x40,
		EX_SetConst = 0x37,
		EX_EndSetConst = 0x38,
		EX_MapConst = 0x39,
		EX_EndMapConst = 0x3a,
		/// <summary>Context expression to address a property within a struct</summary>
		EX_StructMemberContext = 0x44,
		/// <summary>Assignment to a multi-cast delegate</summary>
		EX_LetMulticastDelegate = 0x42,
		/// <summary>Assignment to a delegate</summary>
		EX_LetDelegate = 0x43,
		/// <summary>Special instructions to quickly call a virtual function that we know is going to run only locally</summary>
		EX_LocalVirtualFunction = 0x47,
		/// <summary>Special instructions to quickly call a final function that we know is going to run only locally</summary>
		EX_LocalFinalFunction = 0x48,
		/// <summary>local out (pass by reference) function parameter</summary>
		EX_LocalOutVariable = 0x46,
		EX_DeprecatedOp4A = 0x4b,
		/// <summary>const reference to a delegate or normal function object</summary>
		EX_InstanceDelegate = 0x4f,
		/// <summary>push an address on to the execution flow stack for future execution when a EX_PopExecutionFlow is executed. Execution continues on normally and doesn't change to the pushed address.</summary>
		EX_PushExecutionFlow = 0x50,
		/// <summary>continue execution at the last address previously pushed onto the execution flow stack.</summary>
		EX_PopExecutionFlow = 0x51,
		/// <summary>Goto a local address in code, specified by an integer value.</summary>
		EX_ComputedJump = 0x53,
		/// <summary>continue execution at the last address previously pushed onto the execution flow stack, if the condition is not true.</summary>
		EX_PopExecutionFlowIfNot = 0x52,
		/// <summary>Breakpoint. Only observed in the editor, otherwise it behaves like EX_Nothing.</summary>
		EX_Breakpoint = 0x4a,
		/// <summary>Call a function through a native interface variable</summary>
		EX_InterfaceContext = 0x54,
		/// <summary>Converting an object reference to native interface variable</summary>
		EX_ObjToInterfaceCast = 0x4c,
		/// <summary>Last byte in script code</summary>
		EX_EndOfScript = 0x55,
		/// <summary>Converting an interface variable reference to native interface variable</summary>
		EX_CrossInterfaceCast = 0x4d,
		/// <summary>Converting an interface variable reference to an object</summary>
		EX_InterfaceToObjCast = 0x4e,
		/// <summary>Trace point.  Only observed in the editor, otherwise it behaves like EX_Nothing.</summary>
		EX_WireTracepoint = 0x68,
		/// <summary>A CodeSizeSkipOffset constant</summary>
		EX_SkipOffsetConst = 0x5e,
		/// <summary>Adds a delegate to a multicast delegate's targets</summary>
		EX_AddMulticastDelegate = 0x5f,
		/// <summary>Clears all delegates in a multicast target</summary>
		EX_ClearMulticastDelegate = 0x60,
		/// <summary>Trace point.  Only observed in the editor, otherwise it behaves like EX_Nothing.</summary>
		EX_Tracepoint = 0x69,
		/// <summary>assign to any object ref pointer</summary>
		EX_LetObj = 0x65,
		/// <summary>assign to a weak object pointer</summary>
		EX_LetWeakObjPtr = 0x66,
		/// <summary>bind object and name to delegate</summary>
		EX_BindDelegate = 0x63,
		/// <summary>Remove a delegate from a multicast delegate's targets</summary>
		EX_RemoveMulticastDelegate = 0x61,
		/// <summary>Call multicast delegate</summary>
		EX_CallMulticastDelegate = 0x62,
		EX_LetValueOnPersistentFrame = 0x67,
		EX_ArrayConst = 0x5b,
		EX_EndArrayConst = 0x5c,
		EX_SoftObjectConst = 0x5d,
		/// <summary>static pure function from on local call space</summary>
		EX_CallMath = 0x6a,
		EX_SwitchValue = 0x64,
		/// <summary>Instrumentation event</summary>
		EX_InstrumentationEvent = 0x6b,
		EX_ArrayGetByRef = 0x6c,
		/// <summary>Sparse data variable</summary>
		EX_ClassSparseDataVariable = 0x6d,
		EX_FieldPathConst = 0x5a,
		EX_Max = 0x100,
	};

	public enum ECastToken {
		ObjectToInterface = 0x46,
		ObjectToBool = 0x47,
		InterfaceToBool = 0x49,
		Max = 0xFF,
	};
}
