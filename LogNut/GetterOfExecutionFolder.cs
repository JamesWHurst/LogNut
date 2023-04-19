using Hurst.BaseLib;
using LognutSharedLib;


// Note: This module is not implemented for the Universal Windows Platform, since access to the execution directory is blocked
//       to all uses other than "TrustedInstaller".


namespace Hurst.LogNut
{
    /// <summary>
    /// Class GetterOfExecutionFolder implements <see cref="IGetterOfExecutionFolder"/> 
    /// and provides just the one method <see cref="GetProgramExecutionDirectory"/>.
    /// </summary>
    public class GetterOfExecutionFolder : IGetterOfExecutionFolder
    {
        #region GetProgramExecutionDirectory
        /// <summary>
        /// Return the directory of this executing program, getting it from the assembly.
        /// </summary>
        /// <returns>the directory-name (of the program or assembly) as a string</returns>
        public string GetProgramExecutionDirectory()
        {
            return LognutLib.GetDirectoryProgramIsExecutingIn();
        }
        #endregion
    }
}
