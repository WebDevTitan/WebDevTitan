using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bet365LiveAgent
{
    public partial class FrmSet : Form
    {
        private COMMAND tempCommand = new COMMAND();

        private int nConditionLastSelectedIndex = -1;
        private bool bConditionEdited = false;

        private int nCommandLastSelectedIndex = -1;
        private bool bCommandEdited = false;
        public FrmSet()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBet365Domain.Text))
            {
                MessageBoxEx.Show(this, "Please input bet365 domain!", "Warning", MessageBoxButtons.OK);
                return;
            }
           
            Config.Instance.Bet365Domain = txtBet365Domain.Text;
       
            this.DialogResult = DialogResult.OK;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private int getParsedInt(string value)
        {
            int iTemp = 0;
            bool ok = int.TryParse(value, out iTemp);
            if (!ok)
            {
                iTemp = 0;
			}
            return iTemp;
		}

        private double getParseDouble(string value)
        {
            double dTemp = 0;
            bool ok = double.TryParse(value, out dTemp);
            if (!ok)
            {
                dTemp = 0;
			}
            return dTemp;
		}

        private void EnableControls()
        {
            if (cmbConditionChecker.Text != "" && cmbConditionComparer.Text != "" && txtConditionValue.Text != "" && bConditionEdited)
                btnConditionAdd.Enabled = true;
            else
                btnConditionAdd.Enabled = false;

            if (lstConditions.SelectedIndex != -1 && bConditionEdited)
                btnConditionEdit.Enabled = true;
            else
                btnConditionEdit.Enabled = false;

            if (lstConditions.SelectedIndex != -1)
                btnConditionRemove.Enabled = true;
            else
                btnConditionRemove.Enabled = false;


            if (cmbBetMarket.Text != "" && cmbCommandOddComparer.Text != "" && txtCommandOddValue.Text != "" && txtCommandHandicap.Text != "" && bCommandEdited)
                btnCommandAdd.Enabled = true;
            else
                btnCommandAdd.Enabled = false;

            if (lstCommands.SelectedIndex != -1 && bCommandEdited)
                btnCommandEdit.Enabled = true;
            else
                btnCommandEdit.Enabled = false;

            if (lstCommands.SelectedIndex != -1)
            {
                btnCommandRemove.Enabled = true;
                btnCommandRevertClone.Enabled = true;
            }
            else
            {
                btnCommandRemove.Enabled = false;
                btnCommandRevertClone.Enabled = false;
            }

            
            if (0 <= cmbBetMarket.SelectedIndex && cmbBetMarket.SelectedIndex < Enum.GetValues(typeof(BET_MARKET)).Length)
            {
                if ((BET_MARKET)Enum.GetValues(typeof(BET_MARKET)).GetValue(cmbBetMarket.SelectedIndex) == BET_MARKET.GOAL_LINE)
                {
                    cmbBetOverUnder.Visible = true;
                    cmbBetPlusMinus.Visible = false;
                    cmbBetTeam.Visible = false;
                    cmbBetTeam.Visible = false;
                }
                else
                {
                    cmbBetOverUnder.Visible = false;
                    cmbBetPlusMinus.Visible = true;
                    cmbBetTeam.Visible = true;
                    cmbBetTeam.Visible = true;
                }
            }                
        }
        private void FrmSet_Load(object sender, EventArgs e)
        {
            txtBet365Domain.Text = Config.Instance.Bet365Domain;

            AddConditionCheckerCombo(cmbConditionChecker);
            AddComparisonCombo(cmbConditionComparer);
            AddBetPlusMinusCombo(cmbBetPlusMinus);
            AddBetMarketCombo(cmbBetMarket);
            AddComparisonCombo(cmbCommandOddComparer);
            AddBetOverUnderCombo(cmbBetOverUnder);
            AddBetTeamCombo(cmbBetTeam);

            RefreshCommands();
            EnableControls();
        }

        private void AddConditionCheckerCombo(ComboBox combo)
        {
            combo.Items.Clear();
            foreach (CONDITION_CHECKER value in Enum.GetValues(typeof(CONDITION_CHECKER)))
            {
                combo.Items.Add(value.GetDescription());                
            }
        }

        private void AddComparisonCombo(ComboBox combo)
        {
            combo.Items.Clear();
            foreach (COMPARISON value in Enum.GetValues(typeof(COMPARISON)))
            {
                combo.Items.Add(value.GetDescription());
            }
        }

        private void AddBetPlusMinusCombo(ComboBox combo)
        {
            combo.Items.Clear();
            foreach (BET_PLUSMINUS value in Enum.GetValues(typeof(BET_PLUSMINUS)))
            {
                combo.Items.Add(value.GetDescription());
            }
        }

        private void AddBetMarketCombo(ComboBox combo)
        {
            combo.Items.Clear();
            foreach (BET_MARKET value in Enum.GetValues(typeof(BET_MARKET)))
            {
                combo.Items.Add(value.GetDescription());
            }
        }

        private void AddBetOverUnderCombo(ComboBox combo)
        {
            combo.Items.Clear();
            foreach (BET_OVERUNDER value in Enum.GetValues(typeof(BET_OVERUNDER)))
            {
                combo.Items.Add(value.GetDescription());
            }
        }
        private void AddBetTeamCombo(ComboBox combo)
        {
            combo.Items.Clear();
            foreach (BET_TEAM value in Enum.GetValues(typeof(BET_TEAM)))
            {
                combo.Items.Add(value.GetDescription());
            }
        }

        private void RefreshCommands()
        {
            lstCommands.Items.Clear();

            Config.Instance.CommandsSort();
            foreach (COMMAND command in Config.Instance.Commands)
            {
                lstCommands.Items.Add(command.ToString());
            }

            if (lstCommands.Items.Count > 0 && nCommandLastSelectedIndex < Config.Instance.Commands.Count && nCommandLastSelectedIndex != -1)
            {
                lstCommands.SelectedIndex = nCommandLastSelectedIndex;
            }
            else
            {
                tempCommand = new COMMAND();
                RefreshConditions();

                lstCommands.SelectedIndex = -1;
                               
                cmbBetPlusMinus.Text = "";
                cmbBetTeam.Text = "";
                cmbBetMarket.Text = "";
                cmbBetOverUnder.Text = "";
                txtCommandHandicap.Text = "";
                cmbCommandOddComparer.Text = "";
                txtCommandOddValue.Text = "";
            }

        }
        private void RefreshConditions()
        {
            lstConditions.Items.Clear();
            foreach (CONDITION condition in tempCommand.Conditions)
            {
                lstConditions.Items.Add(condition.ToString());
            }

            if (tempCommand.Conditions.Count > 0 && nConditionLastSelectedIndex < tempCommand.Conditions.Count && nConditionLastSelectedIndex != -1)
            {
                lstConditions.SelectedIndex = nConditionLastSelectedIndex;
            }
            else
            {
                lstConditions.SelectedIndex = -1;

                ConditionInputReset();
            }
        }

        private CONDITION ConditionUpdateData(bool updateInUI)
        {
            if (updateInUI)
            {
                if (cmbConditionComparer.SelectedIndex < 0 || cmbConditionChecker.SelectedIndex < 0 || string.IsNullOrEmpty(txtConditionValue.Text))
                    return null;

                CONDITION condition = new CONDITION();
                foreach (CONDITION_CHECKER value in Enum.GetValues(typeof(CONDITION_CHECKER)))
                {
                    if (cmbConditionChecker.Text == value.GetDescription())
                    {
                        condition.Checker = value;
                        break;
                    }
                }

                foreach (COMPARISON value in Enum.GetValues(typeof(COMPARISON)))
                {
                    if (cmbConditionComparer.Text == value.GetDescription())
                    {
                        condition.Comparer = value;
                        break;
                    }
                }

                condition.Value = txtConditionValue.Text;

                return condition;
            }
            else
            {
                nConditionLastSelectedIndex = lstConditions.SelectedIndex;

                if (nConditionLastSelectedIndex < 0 || nConditionLastSelectedIndex >= tempCommand.Conditions.Count)
                    return null;
                CONDITION condition = tempCommand.Conditions[nConditionLastSelectedIndex];

                for (int i = 0; i < Enum.GetValues(typeof(CONDITION_CHECKER)).Length; i++)
                {
                    if ((CONDITION_CHECKER)Enum.GetValues(typeof(CONDITION_CHECKER)).GetValue(i) == condition.Checker)
                    {
                        cmbConditionChecker.SelectedIndex = i;
                        break;                        
                    }
                }

                for (int i = 0; i < Enum.GetValues(typeof(COMPARISON)).Length; i++)
                {
                    if ((COMPARISON)Enum.GetValues(typeof(COMPARISON)).GetValue(i) == condition.Comparer)
                    {
                        cmbConditionComparer.SelectedIndex = i;
                        break;
                    }
                }

                txtConditionValue.Text = condition.Value;

                return null;
            }
        }
        private void btnConditionAdd_Click(object sender, EventArgs e)
        {
            CONDITION condition = ConditionUpdateData(true);
            if (condition == null)
                return;

            foreach (var itrCond in tempCommand.Conditions)
            {
                if (itrCond == condition)
                    return;
            }
            tempCommand.Conditions.Add(condition);
            bConditionEdited = false;
            bCommandEdited = true;

            nConditionLastSelectedIndex = tempCommand.Conditions.Count - 1;
            RefreshConditions();

            EnableControls(); 
        }

        private void btnConditionEdit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to edit?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            CONDITION condition = ConditionUpdateData(true);
            if (condition == null)
                return;

            
            if (nConditionLastSelectedIndex < 0 || nConditionLastSelectedIndex >= tempCommand.Conditions.Count)
                return;

            tempCommand.Conditions[nConditionLastSelectedIndex] = condition;
            bConditionEdited = false;
            bCommandEdited = true;
            RefreshConditions();

          
            EnableControls();
        }

        private void btnConditionRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to remove?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            int nSelectedIndex = lstConditions.SelectedIndex;

            if (nSelectedIndex < 0 || nSelectedIndex >= tempCommand.Conditions.Count)
                return;
            tempCommand.Conditions.RemoveAt(nSelectedIndex);
            bConditionEdited = false;
            bCommandEdited = true;
            nConditionLastSelectedIndex = -1;
            RefreshConditions();

            EnableControls();
        }

        private void ConditionInputReset()
        {
            cmbConditionChecker.SelectedIndex = -1;
            cmbConditionChecker.Text = "";
            
            cmbConditionComparer.SelectedIndex = -1;
            cmbConditionComparer.Text = "";
            txtConditionValue.Text = "";
        }
        private void lstConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConditionInputReset();
            ConditionUpdateData(false);
                        
            EnableControls();
            bConditionEdited = false;
        }

        private void CommandUpdateData(bool updateInUI)
        {
            if (updateInUI)
            {
                if (cmbBetPlusMinus.SelectedIndex < 0 || cmbBetTeam.SelectedIndex < 0 || cmbBetMarket.SelectedIndex < 0 || cmbCommandOddComparer.SelectedIndex < 0 || string.IsNullOrEmpty(txtCommandOddValue.Text) || string.IsNullOrEmpty(txtCommandHandicap.Text))
                {
                    tempCommand.OddValue = "";
                    return;
                }

                foreach (BET_TEAM value in Enum.GetValues(typeof(BET_TEAM)))
                {
                    if (cmbBetTeam.Text == value.GetDescription())
                    {
                        tempCommand.BetTeam = value;
                        break;
                    }
                }

                foreach (BET_PLUSMINUS value in Enum.GetValues(typeof(BET_PLUSMINUS)))
                {
                    if (cmbBetPlusMinus.Text == value.GetDescription())
                    {
                        tempCommand.BetPlusMinus = value;
                        break;
                    }
                }

                foreach (BET_MARKET value in Enum.GetValues(typeof(BET_MARKET)))
                {
                    if (cmbBetMarket.Text == value.GetDescription())
                    {
                        tempCommand.BetMarket = value;
                        break;
                    }
                }

                foreach (BET_OVERUNDER value in Enum.GetValues(typeof(BET_OVERUNDER)))
                {
                    if (cmbBetOverUnder.Text == value.GetDescription())
                    {
                        tempCommand.BetOverUnder = value;
                        break;
                    }
                }

                tempCommand.Handicap = txtCommandHandicap.Text;

                foreach (COMPARISON value in Enum.GetValues(typeof(COMPARISON)))
                {
                    if (cmbCommandOddComparer.Text == value.GetDescription())
                    {
                        tempCommand.OddComparer = value;
                        break;
                    }
                }

                tempCommand.OddValue = txtCommandOddValue.Text;

                return;
            }
            else
            {
                nCommandLastSelectedIndex = lstCommands.SelectedIndex;

                if (nCommandLastSelectedIndex < 0 || nCommandLastSelectedIndex >= Config.Instance.Commands.Count)
                    return;
                tempCommand = new COMMAND(Config.Instance.Commands[nCommandLastSelectedIndex]);

                for (int i = 0; i < Enum.GetValues(typeof(BET_TEAM)).Length; i++)
                {
                    if ((BET_TEAM)Enum.GetValues(typeof(BET_TEAM)).GetValue(i) == tempCommand.BetTeam)
                    {
                        cmbBetTeam.SelectedIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < Enum.GetValues(typeof(BET_PLUSMINUS)).Length; i++)
                {
                    if ((BET_PLUSMINUS)Enum.GetValues(typeof(BET_PLUSMINUS)).GetValue(i) == tempCommand.BetPlusMinus)
                    {
                        cmbBetPlusMinus.SelectedIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < Enum.GetValues(typeof(BET_MARKET)).Length; i++)
                {
                    if ((BET_MARKET)Enum.GetValues(typeof(BET_MARKET)).GetValue(i) == tempCommand.BetMarket)
                    {
                        cmbBetMarket.SelectedIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < Enum.GetValues(typeof(BET_OVERUNDER)).Length; i++)
                {
                    if ((BET_OVERUNDER)Enum.GetValues(typeof(BET_OVERUNDER)).GetValue(i) == tempCommand.BetOverUnder)
                    {
                        cmbBetOverUnder.SelectedIndex = i;
                        break;
                    }
                }

                txtCommandHandicap.Text = tempCommand.Handicap;

                for (int i = 0; i < Enum.GetValues(typeof(COMPARISON)).Length; i++)
                {
                    if ((COMPARISON)Enum.GetValues(typeof(COMPARISON)).GetValue(i) == tempCommand.OddComparer)
                    {
                        cmbCommandOddComparer.SelectedIndex = i;
                        break;
                    }
                }

                txtCommandOddValue.Text = tempCommand.OddValue;

                RefreshConditions();
                return;
            }
        }
        private void btnCommandAdd_Click(object sender, EventArgs e)
        {
            CommandUpdateData(true);
            
            Config.Instance.Commands.Add(new COMMAND(tempCommand));
            bCommandEdited = false;

            nCommandLastSelectedIndex = Config.Instance.Commands.Count - 1;
            RefreshCommands();

            EnableControls();
        }


        private void btnCommandEdit_Click(object sender, EventArgs e)
        {
            if (sender != null)
            {
                if (MessageBox.Show("Are you sure to edit?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            CommandUpdateData(true);
            if (string.IsNullOrEmpty(tempCommand.OddValue))
                return;

            
            if (nCommandLastSelectedIndex < 0 || nCommandLastSelectedIndex >= Config.Instance.Commands.Count)
                return;

            Config.Instance.Commands[nCommandLastSelectedIndex] = new COMMAND(tempCommand);
            bCommandEdited = false;
            RefreshCommands();

            EnableControls();
        }

        private void btnCommandRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to remove?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            int nSelectedIndex = lstCommands.SelectedIndex;

            if (nSelectedIndex < 0 || nSelectedIndex >= Config.Instance.Commands.Count)
                return;

            Config.Instance.Commands.RemoveAt(nSelectedIndex);
            bCommandEdited = false;
            nCommandLastSelectedIndex = -1;
            RefreshCommands();

            EnableControls();
        }

        private void btnCommandClone_Click(object sender, EventArgs e)
        {

        }

        private void FrmSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bCommandEdited)
            {
                if (MessageBox.Show("Command is updated, will you save it?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    btnCommandEdit_Click(null, null);
                }
            }

            Config.Instance.SaveConfig();
        }

        private void lstCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bCommandEdited)
            {
                if (MessageBox.Show("Command is updated, will you save it?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    btnCommandEdit_Click(null, null);
                }
            }
            nConditionLastSelectedIndex = -1;
            CommandUpdateData(false);
            bCommandEdited = false;
            EnableControls();
            
        }

        private void cmbConditionChecker_SelectedIndexChanged(object sender, EventArgs e)
        {
            bConditionEdited = true;
            EnableControls();
        }

        private void cmbConditionComparer_SelectedIndexChanged(object sender, EventArgs e)
        {
            bConditionEdited = true;
            EnableControls();
        }

        private void txtConditionValue_TextChanged(object sender, EventArgs e)
        {
            bConditionEdited = true;
            EnableControls();
        }

        private void cmbBetSide_SelectedIndexChanged(object sender, EventArgs e)
        {
            bCommandEdited = true;
            EnableControls();
        }

        private void cmbBetMarket_SelectedIndexChanged(object sender, EventArgs e)
        {
            bCommandEdited = true;
            EnableControls();
        }

        private void cmbCommandOddComparer_SelectedIndexChanged(object sender, EventArgs e)
        {
            bCommandEdited = true;
            EnableControls();
        }

        private void txtCommandOddValue_TextChanged(object sender, EventArgs e)
        {
            bCommandEdited = true;
            EnableControls();
        }

        private void cmbBetOverUnder_SelectedIndexChanged(object sender, EventArgs e)
        {
            bCommandEdited = true;
            EnableControls();
        }

        private void txtCommandHandicap_TextChanged(object sender, EventArgs e)
        {
            bCommandEdited = true;
            EnableControls();
        }

        private void btnCommandRevertClone_Click(object sender, EventArgs e)
        {
            
            int nSelectedIndex = lstCommands.SelectedIndex;

            if (nSelectedIndex < 0 || nSelectedIndex >= Config.Instance.Commands.Count)
                return;

            COMMAND temp = new COMMAND(Config.Instance.Commands[nSelectedIndex]);
            temp.Revert();
            
            Config.Instance.Commands.Add(temp);
            bCommandEdited = false;

            nCommandLastSelectedIndex = Config.Instance.Commands.Count - 1;
            RefreshCommands();

            EnableControls();

        }

        private void btnGroupAdd_Click(object sender, EventArgs e)
        {

        }

        private void btnGroupEdit_Click(object sender, EventArgs e)
        {

        }

        private void btnGroupRemove_Click(object sender, EventArgs e)
        {

        }
    }
}
