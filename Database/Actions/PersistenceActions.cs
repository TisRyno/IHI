
using System.Collections.Generic;

namespace IHI.Server.Database.Actions
{
    public static class PersistenceActions
    {
        #region Action: GetPersistentValue
        /// <summary>
        /// Retrieves a persistent value.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="connection">The connection to use. If not specified (or null) then a connection will be picked automatically.</param>
        /// <returns></returns>
        public static byte[] GetPersistentValue(string typeName, long instanceId, string variableName, WrappedMySqlConnection connection = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["@type_name"] = variableName;
            parameters["@instance_id"] = variableName;
            parameters["@variable_name"] = variableName;

            return CoreManager.ServerCore.MySqlConnectionProvider.HelperGetAction<byte[]>("SELECT `value` FROM `persistent_storage` WHERE `type_name` = @type_name AND `instance_id` = @instance_id AND `variable_name` LIMIT 1", parameters, connection);
        }
        #endregion
        #region Action: SetPersistentValue
        /// <summary>
        /// Sets a persistent value.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="connection">The connection to use. If not specified (or null) then a connection will be picked automatically.</param>
        /// <returns></returns>
        public static bool SetPersistentValue(string typeName, long instanceId, string variableName, byte[] value, WrappedMySqlConnection connection = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["@type_name"] = variableName;
            parameters["@instance_id"] = variableName;
            parameters["@variable_name"] = variableName;
            parameters["@value"] = value;

            if (value != null)
                return CoreManager.ServerCore.MySqlConnectionProvider.HelperSetAction("INSERT INTO `persistent_storage` (`type_name`, `instance_id`, `variable_name`, `value`) VALUES (@type_name, @instance_id, @variable_name, @value) ON DUPLICATE KEY UPDATE `value` = @value", parameters, connection);
            return CoreManager.ServerCore.MySqlConnectionProvider.HelperSetAction("DELETE FROM `persistent_storage` WHERE `type_name` = @type_name AND `instance_id` = @instance_id AND `variable_name` = @variable_name", parameters, connection);

        }
        #endregion
    }
}