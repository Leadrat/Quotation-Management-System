"use client";
import { useState } from "react";
import Label from "../form/Label";
import Input from "../form/input/InputField";
import Button from "../ui/button/Button";

interface BankDetails {
  bankDetailsId?: string;
  country: string;
  accountNumber: string;
  ifscCode?: string;
  iban?: string;
  swiftCode?: string;
  bankName: string;
  branchName?: string;
}

interface BankDetailsSectionProps {
  bankDetails: BankDetails[];
  onChange: (bankDetails: BankDetails[]) => void;
}

export default function BankDetailsSection({ bankDetails, onChange }: BankDetailsSectionProps) {
  const [selectedCountry, setSelectedCountry] = useState<string>("");

  const addBankDetails = () => {
    if (!selectedCountry) {
      alert("Please select a country first");
      return;
    }

    // Check if bank details for this country already exist
    if (bankDetails.some(bd => bd.country === selectedCountry)) {
      alert(`Bank details for ${selectedCountry} already exist. Please edit the existing entry.`);
      return;
    }

    const newBankDetail: BankDetails = {
      country: selectedCountry,
      accountNumber: "",
      bankName: "",
      ...(selectedCountry === "India" ? { ifscCode: "" } : { iban: "", swiftCode: "" }),
    };

    onChange([...bankDetails, newBankDetail]);
    setSelectedCountry("");
  };

  const removeBankDetails = (index: number) => {
    onChange(bankDetails.filter((_, i) => i !== index));
  };

  const updateBankDetails = (index: number, field: keyof BankDetails, value: string) => {
    const updated = [...bankDetails];
    updated[index] = { ...updated[index], [field]: value };
    onChange(updated);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Label>Bank Details</Label>
        <div className="flex gap-2">
          <select
            value={selectedCountry}
            onChange={(e) => setSelectedCountry(e.target.value)}
            className="rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm"
          >
            <option value="">Select Country</option>
            <option value="India">India</option>
            <option value="Dubai">Dubai</option>
          </select>
          <Button
            type="button"
            onClick={addBankDetails}
            disabled={!selectedCountry}
            className="bg-brand-600 hover:bg-brand-700 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2"
            aria-label="Add bank details"
          >
            Add Bank Details
          </Button>
        </div>
      </div>

      {bankDetails.map((bd, index) => (
        <div
          key={index}
          className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 space-y-4"
        >
          <div className="flex items-center justify-between mb-4">
            <h4 className="font-semibold text-gray-900 dark:text-white">{bd.country} Bank Details</h4>
            <Button
              type="button"
              onClick={() => removeBankDetails(index)}
              className="bg-red-600 hover:bg-red-700 text-white text-sm px-3 py-1"
            >
              Remove
            </Button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label>Account Number *</Label>
              <Input
                value={bd.accountNumber}
                onChange={(e) => updateBankDetails(index, "accountNumber", e.target.value)}
                placeholder="Enter account number"
                required
              />
            </div>

            <div>
              <Label>Bank Name *</Label>
              <Input
                value={bd.bankName}
                onChange={(e) => updateBankDetails(index, "bankName", e.target.value)}
                placeholder="Enter bank name"
                required
              />
            </div>

            {bd.country === "India" ? (
              <>
                <div>
                  <Label>IFSC Code *</Label>
                  <Input
                    value={bd.ifscCode || ""}
                    onChange={(e) => updateBankDetails(index, "ifscCode", e.target.value)}
                    placeholder="HDFC0001234"
                    required
                  />
                </div>
                <div>
                  <Label>Branch Name</Label>
                  <Input
                    value={bd.branchName || ""}
                    onChange={(e) => updateBankDetails(index, "branchName", e.target.value)}
                    placeholder="Enter branch name"
                  />
                </div>
              </>
            ) : (
              <>
                <div>
                  <Label>IBAN *</Label>
                  <Input
                    value={bd.iban || ""}
                    onChange={(e) => updateBankDetails(index, "iban", e.target.value)}
                    placeholder="AE070331234567890123456"
                    required
                  />
                </div>
                <div>
                  <Label>SWIFT Code *</Label>
                  <Input
                    value={bd.swiftCode || ""}
                    onChange={(e) => updateBankDetails(index, "swiftCode", e.target.value)}
                    placeholder="HDFCINBB"
                    required
                  />
                </div>
                <div>
                  <Label>Branch Name</Label>
                  <Input
                    value={bd.branchName || ""}
                    onChange={(e) => updateBankDetails(index, "branchName", e.target.value)}
                    placeholder="Enter branch name"
                  />
                </div>
              </>
            )}
          </div>
        </div>
      ))}

      {bankDetails.length === 0 && (
        <p className="text-sm text-gray-500 dark:text-gray-400 italic">
          No bank details configured. Add bank details for India or Dubai above.
        </p>
      )}
    </div>
  );
}

