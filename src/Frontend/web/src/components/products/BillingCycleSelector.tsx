"use client";
import type { BillingCycle, BillingCycleMultipliers } from "@/types/products";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";

interface BillingCycleSelectorProps {
  multipliers: BillingCycleMultipliers;
  onChange: (multipliers: BillingCycleMultipliers) => void;
}

export default function BillingCycleSelector({ multipliers, onChange }: BillingCycleSelectorProps) {
  const updateMultiplier = (cycle: keyof BillingCycleMultipliers, value: string) => {
    const numValue = parseFloat(value);
    if (isNaN(numValue) || numValue < 0 || numValue > 1) return;
    onChange({ ...multipliers, [cycle]: numValue });
  };

  return (
    <div className="space-y-4">
      <Label>Billing Cycle Multipliers (0.0 - 1.0)</Label>
      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        <div>
          <Label htmlFor="quarterly">Quarterly</Label>
          <Input
            id="quarterly"
            type="number"
            min="0"
            max="1"
            step="0.01"
            value={multipliers.quarterly ?? 1.0}
            onChange={(e) => updateMultiplier("quarterly", e.target.value)}
            placeholder="0.95"
          />
        </div>
        <div>
          <Label htmlFor="halfYearly">Half-Yearly</Label>
          <Input
            id="halfYearly"
            type="number"
            min="0"
            max="1"
            step="0.01"
            value={multipliers.halfYearly ?? 1.0}
            onChange={(e) => updateMultiplier("halfYearly", e.target.value)}
            placeholder="0.90"
          />
        </div>
        <div>
          <Label htmlFor="yearly">Yearly</Label>
          <Input
            id="yearly"
            type="number"
            min="0"
            max="1"
            step="0.01"
            value={multipliers.yearly ?? 1.0}
            onChange={(e) => updateMultiplier("yearly", e.target.value)}
            placeholder="0.85"
          />
        </div>
        <div>
          <Label htmlFor="multiYear">Multi-Year</Label>
          <Input
            id="multiYear"
            type="number"
            min="0"
            max="1"
            step="0.01"
            value={multipliers.multiYear ?? 1.0}
            onChange={(e) => updateMultiplier("multiYear", e.target.value)}
            placeholder="0.80"
          />
        </div>
      </div>
      <p className="text-sm text-gray-500 dark:text-gray-400">
        Multipliers represent discounts for longer commitments. 1.0 = no discount, 0.95 = 5% discount, etc.
      </p>
    </div>
  );
}

