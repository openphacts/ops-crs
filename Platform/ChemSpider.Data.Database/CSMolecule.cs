using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Data.Database;
using MoleculeObjects;

namespace ChemSpider.Molecules
{
    public class CSMolecule : Molecule2
    {
        private ChemSpiderDB m_db = new ChemSpiderDB();
        private ChemSpiderBlobsDB m_blobsdb = new ChemSpiderBlobsDB();

        /// <summary>
        /// Looks up database to find CSID for standard InChI.
        /// TODO: rationalize this
        /// </summary>
        private int? getCSID()
        {
            return m_db.DBU.ExecuteScalar<int?>(String.Format("select top 1 cmp_id from inchis_std_md5 where inchi_md5 = HashBytes('md5', '{0}')", m_stdInChI));
        }

        public CSMolecule(int cmp_id, bool b3DSDF, bool bCalculate)
        {
            string sdf;
            if (b3DSDF)
            {
                //Try to retrieve an existing 3d file, do not default to 2D, we need to know if it's there.
                sdf = m_blobsdb.getSdf(cmp_id, b3DSDF, false, false);

                //If there is no 3d file in the database then we must call Balloon if we are recalculating.
                if (String.IsNullOrEmpty(sdf) && bCalculate)
                {
                    sdf = ExtUtilsNet.Balloon.opt3d(m_blobsdb.getSdf(cmp_id, false, false));

                    //Update the database with the new sdf - if we managed to create one.
                    if (!String.IsNullOrEmpty(sdf))
                    {
                        m_blobsdb.cmp_set_sdf_3d(cmp_id, sdf, true);
                    }
                    else
                        //Get the 2d file if we couldn't calculate the 3D one.
                        sdf = m_blobsdb.getSdf(cmp_id, false, bCalculate);
                }
            }
            else
                //Get the 2d file.
                sdf = m_blobsdb.getSdf(cmp_id, false, bCalculate);

            //Rebuild using the retrieved or generated sdf.
            this.rebuild(sdf);

            if ( !this.HasProperty("CSID") )
                this.AddProperty("CSID", getCSID().ToString());
        }

        public CSMolecule(int cmp_id)
        {
            this.rebuild(m_blobsdb.getSdf(cmp_id, false, false));

            if ( !this.HasProperty("CSID") )
                this.AddProperty("CSID", getCSID().ToString());
        }
    }
}
